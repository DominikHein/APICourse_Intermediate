namespace APICourse_Intermediate.DTOs
{
    public partial class UserForRegistrationDto
    {

        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }

        public UserForRegistrationDto()
        {

            if (Email == null)
            {

                Email = "";

            }

            if (Password == null)
            {

                Email = "";

            }

            if (PasswordConfirm == null)
            {

                Email = "";

            }

        }

    }
}
