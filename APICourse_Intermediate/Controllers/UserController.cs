using ApiCourse.Data;
using ApiCourse.Model;
using Microsoft.AspNetCore.Mvc;

namespace ApiCourse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        DataContextDapper _dapper;

        public UserController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("TestConnection")]
        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            string sql = @"Select [UserId], " +
                                  "[FirstName], " +
                                  "[LastName], " +
                                  "[Email], " +
                                  "[Gender], " +
                                  "[Active] " +
                           "FROM TutorialAppSchema.Users";

            IEnumerable<User> users = _dapper.LoadData<User>(sql);

            return users;
        }

        [HttpGet("GetUsers/{userId}")]
        public User GetSingleUsers(int userId)
        {

            string sql = @"Select [UserId], " +
                                  "[FirstName], " +
                                  "[LastName], " +
                                  "[Email], " +
                                  "[Gender], " +
                                  "[Active] " +
                           $"FROM TutorialAppSchema.Users WHERE UserId = {userId}";

            User user = _dapper.LoadDataSingle<User>(sql);

            return user;

        }

        [HttpPut("EditUser")]
        public IActionResult EditUser(User user)
        {

            string sql = @"
            Update TutorialAppSchema.Users 
                Set [FirstName] = '" + user.FirstName + @"',
                    [LastName] = '" + user.LastName + @"',
                    [Email] = '" + user.Email + @"',
                    [Gender] = '" + user.Gender + @"',
                    [Active] = '" + user.Active + @"' 
                WHERE UserId = " + user.UserId + ";";

            Console.WriteLine(sql);

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Update User");
        }

        [HttpPost("AddUser")]

        public IActionResult AddUser(UserDto user)
        {
            string sql = @"
                Insert Into TutorialAppSchema.Users 
                    ([FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]) 
                Values (
                    '" + user.FirstName + @"',
                    '" + user.LastName + @"',
                    '" + user.Email + @"',
                    '" + user.Gender + @"',
                    '" + user.Active + @"'
                    )";
            Console.WriteLine(sql);
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Add User");


        }

        [HttpDelete("DeleteSingleUser/{userId}")]

        public IActionResult DeleteUser(int userId)
        {

            string sql = @"
            DELETE FROM TutorialAppSchema.USers WHERE UserId =" + userId.ToString();

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Delete");
        }

    }
}
