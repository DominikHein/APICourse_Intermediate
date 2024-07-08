using ApiCourse.Data;
using APICourse_Intermediate.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICourse_Intermediate.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config)
        {

            _config = config;
            _dapper = new DataContextDapper(config);

        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            //Passwordsalt und PasswordKey kombinieren 
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);
            //PasswordHash generieren 
            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
                );

            return passwordHash;
        }

        public string CreateToken(int userId)
        {
            //Array von Claims erstellen 
            Claim[] claims = new Claim[] {

                new Claim("userId", userId.ToString())

            };


            string? tokenKeyString = _config.GetSection("Appsettings:TokenKey").Value;

            //Symmetrischer Sichherheitsschlüssel generieren 
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    tokenKeyString != null ? tokenKeyString : ""));

            //Anmeldeinformationen erstellen
            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature);

            //SecurityDescriptor erstellen 
            //Enthält Claims, Anmeldeinformationen und Ablaufzeit des Token 
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {
            //Generieren von einem zufälligen Passwordsalt 
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            //SQL Insert 
            string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert 
                                        @Email = @EmailParam, 
                                        @PasswordHash = @PasswordHashParam, 
                                        @PasswordSalt = @PasswordSaltParam"
            ;


            Console.WriteLine(sqlAddAuth);
            //PasswortHash generieren
            byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

            //Erstellen der SQL Parameter für das einfügen vom PasswordHash und Salt
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            emailParameter.Value = userForSetPassword.Email;
            sqlParameters.Add(emailParameter);

            SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary);
            passwordHashParameter.Value = passwordHash;
            sqlParameters.Add(passwordHashParameter);

            SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", SqlDbType.VarBinary);
            passwordSaltParameter.Value = passwordSalt;
            sqlParameters.Add(passwordSaltParameter);



            //Ausführen der SQL 
            return _dapper.ExecuteSqlWitParameters(sqlAddAuth, sqlParameters);

        }
    }
}
