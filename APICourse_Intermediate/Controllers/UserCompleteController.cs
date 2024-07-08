using ApiCourse.Data;
using ApiCourse.Model;
using Microsoft.AspNetCore.Mvc;

namespace ApiCourse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserCompleteController : ControllerBase
    {

        DataContextDapper _dapper;

        public UserCompleteController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("TestConnection")]
        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }


        // Durch Stored Procedure werden GetSingleUser, GetSalary und GetJobInfo Redundant 
        [HttpGet("GetUsersComplete/{userId}/{isActive}")]
        public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
        {
            string sql = $"EXEC TutorialAppSchema.spUsers_Get";

            string parameter = "";


            if (userId != 0)
            {
                parameter += $", @UserId = {userId.ToString()}";
            }
            if (isActive)
            {
                parameter += $", @Active = {isActive}";
            }
            sql += parameter.Substring(1);

            Console.WriteLine(sql);

            IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);

            return users;
        }

        //Insert und Update User mit Stored Procedure 
        //Macht AddUser Redundant
        [HttpPut("Upsert User")]
        public IActionResult EditUser(UserComplete user)
        {

            string sql = @"
            EXEC TutorialAppSchema.spUser_Upsert 
                Set @FirstName = '" + user.FirstName + @"',
                    @LastName = '" + user.LastName + @"',
                    @Email = '" + user.Email + @"',
                    @Gender = '" + user.Gender + @"',
                    @JobTitle = '" + user.JobTitle + @"',
                    @Department = '" + user.Department + @"',
                    @Salary = '" + user.Salary + @"',
                    @Active = '" + user.Active + @"', 
                    @UserId = " + user.UserId + ";";

            Console.WriteLine(sql);

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Update User");
        }

        //Delete Stored Procedure Löscht einträge von Users, UserJobInfo und UserSalary
        [HttpDelete("DeleteSingleUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {

            string sql = $"TutorialAppSchema.spUser_Delete, @{userId.ToString()}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Delete");
        }

    }
}
