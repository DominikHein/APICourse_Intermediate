namespace APICourse_Intermediate.DTOs
{
    public class PostToAddDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }
        public DateTime PostCreated { get; set; }
        public DateTime PostUpdated { get; set; }


        public PostToAddDto()
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
