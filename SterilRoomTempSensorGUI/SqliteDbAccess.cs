using System;
using System.Collections.Generic;
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

        IDbConnection cnn;
        public SqliteDbAccess()
        {
            Current = this;
            cnn = new SQLiteConnection(LoadConnString());
            cnn.Open();
            var output = (string)cnn.ExecuteScalar("SELECT SQLITE_VERSION()");
            Debug.WriteLine("Connected to local db, using SQLite ver " + output);
        }

        string LoadConnString(string id = "Default") => ConfigurationManager.ConnectionStrings[id].ConnectionString;

        public void SaveDataSuhu(int index_sensor, DateTime time, double suhu, string keterangan = null)
        {
            var param = new DynamicParameters();
            param.Add("UnixEpoch", new DateTimeOffset(time).ToUnixTimeMilliseconds());
            param.Add("Suhu", suhu);
            param.Add("Keterangan", keterangan);
            cnn.Execute($"insert into Sensor{index_sensor} (unixepoch, suhu, keterangan) values (@UnixEpoch, @Suhu, @Keterangan)", param);
        }
    }
}
