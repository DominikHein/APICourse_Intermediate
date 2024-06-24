namespace ApiCourse.Model
{


    public partial class UserDto
    {
        //Data Transfer Object für User Tabelle 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }

        public UserDto()
        {
            if (FirstName == null)
            {
                FirstName = "";
            }

            if (LastName == null)
            {
                FirstName = "";
            }

            if (Email == null)
            {
                FirstName = "";
            }

            if (Gender == null)
            {
                FirstName = "";
            }
        }

    }


}
