using ApiCourse.Data;
using APICourse_Intermediate.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace APICourse_Intermediate.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }
        //Post aus Datenbank holen
        //Mit Stored Procedure nach PostId, UserId oder Search Parameter Filtern
        //Macht PostSingle, PostByUser und PostBySearch Redundant
        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string stringParameter = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            //Wenn 0 oder None wird nicht danach gefiltert 
            if (postId != 0)
            {
                stringParameter += $", @PostId = @PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postId, System.Data.DbType.Int32);
            }
            if (userId != 0)
            {
                stringParameter += $", @UserId = {userId.ToString()}";
                sqlParameters.Add("@UserIdParameter", userId, System.Data.DbType.Int32);
            }

            if (searchParam != "None")
            {
                stringParameter += $", @SearchValue = {searchParam.ToString()}";
                sqlParameters.Add("@SearchValueParameter", searchParam, System.Data.DbType.String);
            }

            if (stringParameter.Length > 0)
            {
                sql += stringParameter.Substring(1);
            }

            Console.WriteLine(sql);

            return _dapper.LoadDataWithParameter<Post>(sql, sqlParameters);
        }



        /*
        //Bestimmten Post aus datenbank holen 
        [HttpGet("PostSingle/{postId}")]
        public IEnumerable<Post> GetPostSingle(int postId)
        {
            string sql = @"SELECT [PostId],
                                  [UserId],
                                  [PostTitle],
                                  [PostContent],
                                  [PostCreated],
                                  [PostUpdated] FROM TutorialAppSchema.Post
                                  WHERE PostId = " + postId.ToString();

            return _dapper.LoadData<Post>(sql);
        } */

        /*
        //Post von einem bestimmten Nutzer aus der Datenbank holen 
        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostByUser(int userId)
        {
            string sql = @"SELECT [PostId],
                                  [UserId],
                                  [PostTitle],
                                  [PostContent],
                                  [PostCreated],
                                  [PostUpdated] FROM TutorialAppSchema.Post
                                  WHERE PostId = " + userId.ToString();

            return _dapper.LoadData<Post>(sql);
        }
        */


        //Posts des derzeitigen Users aus Datenbank holen 
        //Benutzer aus dem derzeitigen Token wird genommen
        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParameter";
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, System.Data.DbType.Int32);
            Console.WriteLine(User.FindFirst("userId")?.Value);


            return _dapper.LoadDataWithParameter<Post>(sql, sqlParameters);
        }


        /*
        //Post nach Inhalt suchen 
        [HttpGet("PostBySearch/{searchParam}")]
        public IEnumerable<Post> PostBySearch(string searchParam)
        {
            string sql = @"SELECT [PostId],
                                  [UserId],
                                  [PostTitle],
                                  [PostContent],
                                  [PostCreated],
                                  [PostUpdated] FROM TutorialAppSchema.Post
                                  WHERE PostTitle LIKE '%" + searchParam + "%'" +
                                  "OR PostContent LIKE '%" + searchParam + "%'";

            Console.WriteLine(sql);

            return _dapper.LoadData<Post>(sql);
        }
        */


        //Post zur Datenbank hinzufügen
        //Update oder Insert Post 
        //Edit Post Redundant
        [HttpPut("UpsertPost")]
        public IActionResult AddPost(Post postToUpsert)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_Upsert 
                         @UserId = @UserIdParameter,
                         @PostTitle = @PostTitleParameter,
                         @PostContent = @PostContentParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, System.Data.DbType.Int32);
            sqlParameters.Add("@PostTitleParameter", postToUpsert.PostTitle, System.Data.DbType.String);
            sqlParameters.Add("@PostContentParameter", postToUpsert.PostContent, System.Data.DbType.String);

            if (postToUpsert.PostId > 0)
            {
                sql += ", @PostId = @PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postToUpsert.PostId, System.Data.DbType.Int32);
            }

            Console.WriteLine(sql);
            Console.WriteLine(User.FindFirst("userId")?.Value);

            if (_dapper.ExecuteSqlWitParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to create post");

        }

        //Post in DB Editieren
        [HttpPost("EditPost")]
        public IActionResult EditPost(Post postToEdit)
        {
            //SQL das den Post nach der Mitgegebenen PostId durchsucht und "prüft" ob der Post von dem Benutzer 
            //erstellt worden ist der die Anfrage schickt
            string sql = @"Update TutorialAppSchema.Post 
                         SET PostContent = '" + postToEdit.PostContent
                         + "', PostTitle = '" + postToEdit.PostTitle
                         + @"', PostUpdated = GETDATE()
                         WHERE PostId = " + postToEdit.PostId.ToString() +
                         "AND UserId = " + User.FindFirst("userId")?.Value;
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to edit post");

        }

        //Post löschen 
        [HttpDelete("PostDelete/{postId}")]
        public IActionResult DeletePost(int postId)
        {

            string sql = $"EXEC TutorialAppSchema.spPost_Delete @PostId = @PostIdParameter, @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@PostIdParameter", postId, System.Data.DbType.Int32);
            sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, System.Data.DbType.Int32);

            if (_dapper.ExecuteSqlWitParameters(sql, sqlParameters))
            {
                return Ok();
            }
            throw new Exception("Löschen Fehlgeschlagen");
        }

    }
}
