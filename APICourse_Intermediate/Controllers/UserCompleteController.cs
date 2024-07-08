using ApiCourse.Data;
using ApiCourse.Model;
using APICourse_Intermediate.Helpers;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiCourse.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserCompleteController : ControllerBase
    {

        DataContextDapper _dapper;
        ReusableSql _reusableSql;

        public UserCompleteController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _reusableSql = new ReusableSql(config);
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

            string stringParameter = "";

            DynamicParameters sqlParameters = new DynamicParameters();


            if (userId != 0)
            {
                stringParameter += $", @UserId = @UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, System.Data.DbType.Int32);

            }
            if (isActive)
            {
                stringParameter += $", @Active = @ActiveParameter";
                sqlParameters.Add("@ActiveParameter", isActive, System.Data.DbType.Boolean);
            }
            if (stringParameter.Length != 0)
            {
                sql += stringParameter.Substring(1);
            }


            Console.WriteLine(sql);

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameter<UserComplete>(sql, sqlParameters);

            return users;
        }

        //Insert und Update User mit Stored Procedure 
        //Macht AddUser Redundant
        [HttpPut("Upsert User")]
        public IActionResult UpsertUser(UserComplete user)
        {
            if (_reusableSql.UpsertUser(user))
            {
                return Ok();
            }

            throw new Exception("Failed to Update User");
        }

        //Delete Stored Procedure Löscht einträge von Users, UserJobInfo und UserSalary
        [HttpDelete("DeleteSingleUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {

            string sql = $"TutorialAppSchema.spUser_Delete @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@UserIdParameter", userId, System.Data.DbType.Int32);

            if (_dapper.ExecuteSqlWitParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to Delete");
        }

    }
}
