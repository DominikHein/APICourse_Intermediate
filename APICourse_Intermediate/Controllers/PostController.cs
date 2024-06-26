using ApiCourse.Data;
using APICourse_Intermediate.DTOs;
using APICourse_Intermediate.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICourse_Intermediate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }
        //Alle Posts aus Datenbank holen
        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                                  [UserId],
                                  [PostTitle],
                                  [PostContent],
                                  [PostCreated],
                                  [PostUpdated] FROM TutorialAppSchema.Post";

            return _dapper.LoadData<Post>(sql);
        }
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
        }
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
        //Posts des derzeitigen Users aus Datenbank holen 
        //Benutzer aus dem derzeitigen Token wird genommen
        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"SELECT [PostId],
                                  [UserId],
                                  [PostTitle],
                                  [PostContent],
                                  [PostCreated],
                                  [PostUpdated] FROM TutorialAppSchema.Post
                                  WHERE UserId = " + User.FindFirst("userId")?.Value;

            Console.WriteLine(sql);

            return _dapper.LoadData<Post>(sql);
        }
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


        //Post zur Datenbank hinzufügen
        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sql = @"Insert Into TutorialAppSchema.Post (
                        [UserId],
                        [PostTitle],
                        [PostContent],
                        [PostCreated],
                        [PostUpdated]) VALUES (" + User.FindFirst("userId")?.Value
                        + ",'" + postToAdd.PostTitle
                        + "','" + postToAdd.PostContent
                        + "', GETDATE(), GETDATE() )";
            Console.WriteLine(sql);
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to create post");

        }

        //Post in DB Editieren
        [HttpPost("EditPost")]
        public IActionResult EditPost(PostToEditDto postToEdit)
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

            string sql = @"DELETE FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString();

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Löschen Fehlgeschlagen");
        }

    }
}
