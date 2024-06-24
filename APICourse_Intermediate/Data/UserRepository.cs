using ApiCourse.Data;
using ApiCourse.Model;

namespace APICourse_Intermediate.Data
{
    public class UserRepository : IUserRepository
    {

        DataContextEF _entityFramework;

        public UserRepository(IConfiguration config)
        {

            _entityFramework = new DataContextEF(config);

        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);

            }
        }

        public void RemoveEntity<T>(T entitytoRemove)
        {
            if (entitytoRemove != null)
            {
                _entityFramework.Remove(entitytoRemove);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entityFramework.Users
                .Where(u => u.UserId == userId)
                .FirstOrDefault<User>();

            if (user != null)
            {
                return user;
            }

            throw new Exception("Failed to Get User");
        }

        public UserSalary GetSingleUserSalary(int userId)
        {
            UserSalary? userSalary = _entityFramework.UsersSalary
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userSalary != null)
            {
                return userSalary;
            }

            throw new Exception("Failed to Get User");
        }

        public UserJobInfo GetSingleUserJobInfo(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramework.UsersJobInfo
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userJobInfo != null)
            {
                return userJobInfo;
            }

            throw new Exception("Failed to Get User");
        }

    }
}
