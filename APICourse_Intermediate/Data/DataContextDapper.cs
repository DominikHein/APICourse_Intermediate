using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiCourse.Data
{
    public class DataContextDapper
    {

        private readonly IConfiguration _configuration;
        public DataContextDapper(IConfiguration config)
        {
            _configuration = config;
        }

        //Methode um mehrere Daten aus der Datenbank abzufragen
        public IEnumerable<T> LoadData<T>(string sql)
        {
            //Connection kreieren 
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //Dappers Query um SQL zu executen 
            return dbConnection.Query<T>(sql);



        }
        //Methode um einzelenen Eintrag abzufragen 
        public T LoadDataSingle<T>(string sql)
        {

            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);



        }
        //Methode für SQL Execute 
        public bool ExecuteSql(string sql)
        {

            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //Gibt zurück ob sich was geändert hat 
            return dbConnection.Execute(sql) > 0;

        }
        //Methode für SQL Execute 
        public int ExecuteSqlWithRowCount(string sql)
        {

            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //Gibt anzahl der Zeilen an die geändert worden sind
            return dbConnection.Execute(sql);

        }



    }
}
