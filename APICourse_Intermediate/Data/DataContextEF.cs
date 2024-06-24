using ApiCourse.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiCourse.Data
{
    public class DataContextEF : DbContext
    {

        private readonly IConfiguration _configuration;
        internal IEnumerable<object> userSalary;

        public DataContextEF(IConfiguration config)
        {

            _configuration = config;

        }
        //"Interne Tabelle" von meinen Dbs 
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserSalary> UsersSalary { get; set; }

        public virtual DbSet<UserJobInfo> UsersJobInfo { get; set; }

        //Datenbank Verbindung Konfigurieren
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"), //Default Connection aus appsettings.json
                    optionsBuilder => optionsBuilder.EnableRetryOnFailure()); // Bei Verbindungsfehler automatisch neu versuchen zu verbinden
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema"); //Festlegen welches Schema verwerndet werden soll 

            modelBuilder.Entity<User>().ToTable("Users", "TutorialAppSchema").HasKey(u => u.UserId); //Mappt die Entität des User Model auf die Users Tabelle und legt Primary Key fest

            modelBuilder.Entity<UserSalary>().HasKey(u => u.UserId); // Legt Primary Key fest

            modelBuilder.Entity<UserJobInfo>().HasKey(u => u.UserId); // Legt Primary Key fest
        }


    }
}
