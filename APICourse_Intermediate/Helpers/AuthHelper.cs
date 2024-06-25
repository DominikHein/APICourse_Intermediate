using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APICourse_Intermediate.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        public AuthHelper(IConfiguration config)
        {

            _config = config;

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

    }
}
