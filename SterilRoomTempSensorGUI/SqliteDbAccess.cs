using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using OxyPlot;

namespace SterilRoomTempSensorGUI
{
    public class SqliteDbAccess
    {
        public static SqliteDbAccess Current { get; private set; }
        string sqlInsert = "insert into DataSuhu ";

        public SqliteDbAccess()
        {
            Current = this;
        }

        string LoadConnString(string id = "Default") => ConfigurationManager.ConnectionStrings[id].ConnectionString;

        public void SaveDataSuhu(DataSuhu dataSuhu)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnString()))
            {
                var sqlStatement = "insert into DataSuhu (Sensor1,Sensor2,Sensor3,Sensor4,Sensor5,Sensor6,Rata2,Waktu)" +
                    "values (@Sensor1,@Sensor2,@Sensor3,@Sensor4,@Sensor5,@Sensor6,@Rata2,@Waktu)";
                cnn.ExecuteAsync(sqlStatement, dataSuhu);
            }
        }

        public List<DataSuhu> LoadDatalistSuhu(DateTime startTime, DateTime endTime)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnString()))
            {
                return cnn.Query<DataSuhu>("SELECT * from DataSuhu where Waktu >= @StartDate and Waktu <= @EndDate",
                    new { StartDate = new DateTimeOffset(startTime).ToUnixTimeMilliseconds(), EndDate = new DateTimeOffset(endTime).ToUnixTimeMilliseconds() }).ToList();
            }
        }
    }
}
