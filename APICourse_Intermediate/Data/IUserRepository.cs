﻿using ApiCourse.Model;

namespace APICourse_Intermediate.Data
{
    public interface IUserRepository
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entitytoRemove);

        public IEnumerable<User> GetUsers();

        public User GetSingleUser(int userId);
        public UserSalary GetSingleUserSalary(int userId);
        public UserJobInfo GetSingleUserJobInfo(int userId);
    }
}