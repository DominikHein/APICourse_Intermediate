﻿namespace APICourse_Intermediate.DTOs
{
    public partial class UserForRegistrationDto
    {

        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public double Salary { get; set; }


        public UserForRegistrationDto()
        {

            if (Email == null)
            {
                Email = "";
            }

            if (Password == null)
            {
                Password = "";
            }

            if (PasswordConfirm == null)
            {
                PasswordConfirm = "";
            }

            if (FirstName == null)
            {
                FirstName = "";
            }

            if (LastName == null)
            {
                FirstName = "";
            }

            if (Gender == null)
            {
                FirstName = "";
            }

            if (JobTitle == null)
            {
                JobTitle = "";
            }

            if (Department == null)
            {
                Department = "";
            }

        }

    }
}
