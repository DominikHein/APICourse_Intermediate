using ApiCourse.Data;
using ApiCourse.Model;
using Dapper;

namespace APICourse_Intermediate.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration configuration)
        {

            _dapper = new DataContextDapper(configuration);

        }

        public bool UpsertUser(UserComplete user)
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

            return (_dapper.ExecuteSqlWitParameters(sql, sqlParameters));

        }
    }
}
