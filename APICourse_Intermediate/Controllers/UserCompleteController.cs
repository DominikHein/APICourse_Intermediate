using ApiCourse.Data;
using ApiCourse.Model;
using Dapper;
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
        public IActionResult EditUser(UserComplete user)
        {

            string sql = @"
            EXEC TutorialAppSchema.spUser_Upsert 
                Set @FirstName = @FirstNameParameter,
                    @LastName = @LastNameParameter,
                    @Email = @EmailParameter,
                    @Gender = @GenderParameter,
                    @JobTitle = @JobTitleParameter,
                    @Department = @DepartmentParameter,
                    @Salary = @SalaryParameter,
                    @Active = @ActiveParameter, 
                    @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@FirstNameParameter", user.FirstName, System.Data.DbType.String);
            sqlParameters.Add("@LastNameParameter", user.LastName, System.Data.DbType.String);
            sqlParameters.Add("@EmailParameter", user.Email, System.Data.DbType.String);
            sqlParameters.Add("@GenderParameter", user.Gender, System.Data.DbType.String);
            sqlParameters.Add("@JobTitleParameter", user.JobTitle, System.Data.DbType.String);
            sqlParameters.Add("@DepartmentParameter", user.Department, System.Data.DbType.String);
            sqlParameters.Add("@SalaryParameter", user.Salary, System.Data.DbType.Decimal);
            sqlParameters.Add("@ActiveParameter", user.Active, System.Data.DbType.Boolean);
            sqlParameters.Add("@UserIdParameter", user.UserId, System.Data.DbType.Int32);

            Console.WriteLine(sql);

            if (_dapper.ExecuteSqlWitParameters(sql, sqlParameters))
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
