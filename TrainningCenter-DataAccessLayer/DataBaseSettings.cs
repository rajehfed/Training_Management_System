using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_DataAccessLayer
{
    public class DataBaseSettings
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["TrainingCenterDB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
