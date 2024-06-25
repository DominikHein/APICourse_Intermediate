namespace APICourse_Intermediate.DTOs
{
    public class PostToEditDto
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }

        public PostToEditDto()
        {

            if (PostTitle == null)
            {
                PostTitle = "";
            }

            if (PostContent == null)
            {
                PostContent = "";
            }

        }

    }
}
