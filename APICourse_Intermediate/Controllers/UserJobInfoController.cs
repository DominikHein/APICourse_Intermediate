using ApiCourse.Data;
using ApiCourse.Model;
using Microsoft.AspNetCore.Mvc;

namespace ApiCourse.Controllers
{
    public class UserJobInfoController : ControllerBase
    {

        DataContextDapper _dapper;

        public UserJobInfoController(IConfiguration configuration)
        {

            _dapper = new DataContextDapper(configuration);

        }


        //Liste von allen Job Infos abrufen 
        [HttpGet("GetJobInfo")]
        public IEnumerable<UserJobInfo> GetUserJobInfos()
        {
            string sql = @"SELECT [UserId]
                                 ,[JobTitle]
                                 ,[Department]
                                 FROM TutorialAppSchema.UserJobInfo";

            IEnumerable<UserJobInfo> userJobInfo = _dapper.LoadData<UserJobInfo>(sql);

            return userJobInfo;

        }
        //Einzelnes Job Info Objekt abrufen mit User ID 
        [HttpGet("GetJobInfoSingle/{userId}")]
        public UserJobInfo GetUserJobSingle(int userId)
        {

            string sql = @"SELECT [UserId]
                                 ,[JobTitle]
                                 ,[Department]
                                 FROM TutorialAppSchema.UserJobInfo WHERE UserId = " + userId;

            return _dapper.LoadDataSingle<UserJobInfo>(sql);

        }
        //Mit JobInfo Objekt eintrag in DB ändern
        [HttpPut("EditJobInfo")]
        public IActionResult EditJobInfo(UserJobInfo userJobInfo)
        {

            string sql = @"Update TutorialAppSchema.UserJobInfo
                            Set [JobTitle] = '" + userJobInfo.JobTitle + @"',
                                [Department] = '" + userJobInfo.Department + @"' 
                                WHERE UserId = " + userJobInfo.UserId + ";";

            Console.WriteLine(sql);


            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Edit User");

        }

        //JobInfo Objekt hinzufügen 
        [HttpPost("AddJobInfo")]
        public IActionResult AddJobInfo(UserJobInfo userJobInfo)
        {
            string sql = @"Insert Into TutorialAppSchema.UserJobInfo
                          ([userID],
                           [JobTitle],
                           [Department])
                           Values(
                           '" + userJobInfo.UserId + @"',
                           '" + userJobInfo.JobTitle + @"',
                           '" + userJobInfo.Department + @"')";

            Console.WriteLine(sql);

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to Add Job Info");
        }


        //JobInfo Eintrag löschen mit userID
        [HttpDelete("UserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfo(int userId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.UserJobInfo 
                WHERE UserId = " + userId.ToString();

            Console.WriteLine(sql);

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to Delete Job Info");
        }



    }
}
