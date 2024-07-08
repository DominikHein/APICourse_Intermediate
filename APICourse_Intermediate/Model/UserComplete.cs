namespace ApiCourse.Model
{


    public partial class UserComplete
    {

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public double Salary { get; set; }

        public UserComplete()
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
