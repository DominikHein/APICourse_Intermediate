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
                                  WHERE PostId = " + User.FindFirst("userId")?.Value;

            return _dapper.LoadData<Post>(sql);
        }


        //Post zur Datenbank hinzufügen
        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sql = @"Insert Into TutorialAppSchema.Post [PostId],
                        [UserId],
                        [PostTitle],
                        [PostContent],
                        [PostCreated],
                        [PostUpdated] VALUES (" + User.FindFirst("userId")?.Value
                        + ",'" + postToAdd.PostTitle
                        + ",'" + postToAdd.PostContent
                        + "', GETDATE(), GETDATE() )";
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to create post");

        }
    }
}
