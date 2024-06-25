using ApiCourse.Data;
using APICourse_Intermediate.DTOs;
using APICourse_Intermediate.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;

namespace APICourse_Intermediate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {

            _dapper = new DataContextDapper(config);

            _authHelper = new AuthHelper(config);


        }
        [AllowAnonymous]
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
                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

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
        [AllowAnonymous]
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
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);
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
            //Token zurückgeben
            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            }
            );
        }


        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            //Benutzer ID aus den Claims abrufen
            string userId = User.FindFirst("userId")?.Value + "";
            //SQL abfrage um Benutzer ID aus DB abzurufen
            string userIdSql = $"SELECT userId FROM TutorialAppSchema.Users WHERE UserId = '{userId}'";
            //Benutzer ID abrufen 
            int userIdFromDB = _dapper.LoadDataSingle<int>(userIdSql);

            //Rückgabe eines neuen Token
            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userIdFromDB)} });
        }



    }
}
