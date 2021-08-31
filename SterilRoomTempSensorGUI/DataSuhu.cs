using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SterilRoomTempSensorGUI
{
    public class DataSuhu
    {
        public long ID { get; set; }
        public double Sensor1 { get; set; }
        public double Sensor2 { get; set; }
        public double Sensor3 { get; set; }
        public double Sensor4 { get; set; }
        public double Sensor5 { get; set; }
        public double Sensor6 { get; set; }
        public double Rata2 { get { return new List<double>() { Sensor1, Sensor2, Sensor3, Sensor4, Sensor5, Sensor6 }.Average(); } }

        public long Waktu { get { return new DateTimeOffset(base_waktu).ToUnixTimeMilliseconds(); } set { base_waktu = DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime; } }
        public DateTime base_waktu { get; private set; }

        public DataSuhu() { }

        public DataSuhu(List<double> suhus, DateTime _waktu)
        {
            Sensor1 = suhus[0];
            Sensor2 = suhus[1];
            Sensor3 = suhus[2];
            Sensor4 = suhus[3];
            Sensor5 = suhus[4];
            Sensor6 = suhus[5];

            base_waktu = _waktu;
        }
    }
}
