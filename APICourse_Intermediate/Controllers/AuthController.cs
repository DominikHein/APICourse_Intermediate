using ApiCourse.Data;
using APICourse_Intermediate.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICourse_Intermediate.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {

            _dapper = new DataContextDapper(config);
            _config = config;

        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            //Passwort und bestätigungs Passwort gleich?  
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                //SELECT Email um nur ein Set of Strings zurückzubekommen 
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";

                //SQL ausführen 
                IEnumerable<String> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

                //Wenn was zurück kommt ist die Mail bereits verwendet 
                if (existingUsers.Count() == 0)
                {
                    //Generieren von einem zufälligen Passwordsalt 
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    //SQL Insert 
                    string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth ([Email],
                                        [PasswordHash],
                                        [PasswordSalt]) VALUES ('" + userForRegistration.Email + "'" +
                                        ", @PasswordHash, @PasswordSalt)";

                    Console.WriteLine(sqlAddAuth);
                    //PasswortHash generieren
                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    //Erstellen der SQL Parameter für das einfügen vom PasswordHash und Salt
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;


                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);


                    //Ausführen der SQL 
                    if (_dapper.ExecuteSqlWitParameters(sqlAddAuth, sqlParameters))
                    {
                        //Nutzer hinzufügen wenn Registriert 
                        string sqlAddUser = @"
                                            Insert Into TutorialAppSchema.Users 
                                            ([FirstName],
                                            [LastName],
                                            [Email],
                                            [Gender],
                                            [Active]) 
                                            Values (
                                            '" + userForRegistration.FirstName + @"',
                                            '" + userForRegistration.LastName + @"',
                                            '" + userForRegistration.Email + @"',
                                            '" + userForRegistration.Gender + @"',
                                            1)";
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }

                        throw new Exception("Failed to Add User");
                    }


                    throw new Exception("Failed to register user");
                }
                throw new Exception("User with this Email already exists");

            }

            throw new Exception("Passwords do not match");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            //Hash und Salt zur Email bekommen 
            string sqlForHashAndSalt = @"SELECT 
                                        [PasswordHash],
                                        [PasswordSalt] FROM TutorialAppSchema.Auth 
                                        WHERE Email ='" + userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);
            //Passwordhash generieren
            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);
            //Hashes vergleichen 
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect Password");
                }
            }

            string userIdSql = $"SELECT * FROM TutorialAppSchema.USERS WHERE Email = '{userForLogin.Email}'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", CreateToken(userId)}
            }
            );
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
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

        private string CreateToken(int userId)
        {
            //Array von Claims erstellen 
            Claim[] claims = new Claim[] {

                new Claim("userId", userId.ToString())

            };

            //Symmetrischer Sichherheitsschlüssel generieren 
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config.GetSection("Appsettings:TokenKey").Value));

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
    }
}
