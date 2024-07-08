using ApiCourse.Data;
using ApiCourse.Model;
using APICourse_Intermediate.DTOs;
using APICourse_Intermediate.Helpers;
using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace APICourse_Intermediate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        private readonly AuthHelper _authHelper;

        private readonly IMapper _mapper;

        private readonly ReusableSql _reusableSql;

        public AuthController(IConfiguration config)
        {

            _dapper = new DataContextDapper(config);

            _authHelper = new AuthHelper(config);

            _reusableSql = new ReusableSql(config);

            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
            }));


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
                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password,
                    };
                    //Ausführen der SQL 
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        //userForRegistraion Felder auf die userComplete Felder mappen 
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;

                        if (_reusableSql.UpsertUser(userComplete))
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

        [HttpPut("ResetPassword")]

        public IActionResult ResetPassword(UserForLoginDto userForResetPassword)
        {
            if (_authHelper.SetPassword(userForResetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to Update Password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            //Hash und Salt zur Email bekommen 
            string sqlForHashAndSalt = @"Exec TutorialAppSchema.spLoginConfirmation_Get
                                       @Email = @EmailParam";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingleWithParameter<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);
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
