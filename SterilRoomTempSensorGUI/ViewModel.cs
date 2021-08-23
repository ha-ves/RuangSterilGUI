using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SterilRoomTempSensorGUI
{
    public static class ExtensionMethods
    {
        public static double Remap(this double value, float from_min, float from_max, float to_min, float to_max)
        {
            return (value - from_min) / (from_max - from_min) * (to_max - to_min) + to_min;
        }
    }

    class ViewModel
    {
        public static PlotModel ChartModel { get; private set; }

        const string HMSFormat = "HH\\:mm\\:ss";
        List<LineSeries> SensorSuhuSeries { get; set; } = new List<LineSeries>(6);
        DateTime StartRecordDate { get; set; }
        LineAnnotation CurrTimeAnnotation { get; set; }

        Timer digitalClockTimer, dataDummyTimer;
        public ViewModel()
        {
            StartRecordDate = DateTime.Now;

            ChartModel = new PlotModel()
            {
                Title = "Temperatur Ruang Sterilisasi",
                Subtitle = StartRecordDate.ToString($"DISCONNECTED - {HMSFormat} - dd / MMMM / yyyy", CultureInfo.GetCultureInfo("id-ID")),
                TitleFontSize = 24,
                SubtitleFontSize = 20,
                PlotAreaBorderColor = OxyColors.Gray,
                SubtitleColor = OxyColors.DarkGreen,
                SubtitleFontWeight = 500,
                DefaultFontSize = 20,
                LegendBackground = OxyColors.White,
                LegendLineSpacing = 5,
                LegendFontSize = 20,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
            };
            ChartModel.Axes.Add(new LinearAxis()
            {
                Title = "Suhu (Celcius)",
                StringFormat = "0°C",
                Position = AxisPosition.Left,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 50,
                MinimumRange = 50,
                IsZoomEnabled = false,
                MajorStep = 10,
                MinorStep = 2,
                MajorGridlineStyle = LineStyle.Automatic,
                MajorGridlineThickness = 1.5,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineStyle = LineStyle.Automatic,
                MinorGridlineThickness = 0.5,
                MinorGridlineColor = OxyColors.LightGray,
            });
            ChartModel.Axes.Add(new DateTimeAxis()
            {
                Title = "Waktu Sistem",
                StringFormat = HMSFormat,
                Position = AxisPosition.Bottom,
                IntervalType = DateTimeIntervalType.Seconds,
                //IntervalLength = 36,
                Angle = -45,
                AbsoluteMinimum = Axis.ToDouble(StartRecordDate),
                MinimumRange = 0.0005,
                MaximumRange = 1,
                MajorGridlineStyle = LineStyle.Automatic,
                MajorGridlineThickness = 1.5,
                MajorGridlineColor = OxyColors.LightGray,
            });
            ChartModel.Axes[1].Zoom(Axis.ToDouble(StartRecordDate), Axis.ToDouble(StartRecordDate + TimeSpan.FromMinutes(1)));

            for (int i = 0; i < 6; i++)
            {
                SensorSuhuSeries.Add(new LineSeries()
                {
                    Title = $"Sensor {i + 1}",
                    TrackerFormatString = "Data {0}\n" +
                                            "Suhu : {4:0.##}°C\n" +
                                            "Pukul {2:" + HMSFormat + "}\n",
                    LineStyle = (LineStyle)i
                });
                ChartModel.Series.Add(SensorSuhuSeries[i]);
            }

            CurrTimeAnnotation = new LineAnnotation()
            {
                Type = LineAnnotationType.Vertical,
                X = Axis.ToDouble(StartRecordDate),
            };
            ChartModel.Annotations.Add(CurrTimeAnnotation);

            digitalClockTimer = new Timer(500);
            digitalClockTimer.Elapsed += DigitalClockRefresh;
            digitalClockTimer.Start();

            dataDummyTimer = new Timer(2000);
            dataDummyTimer.Elapsed += AddData;
            dataDummyTimer.Start();
        }

        Random randomizer = new Random(67676767);
        private void AddData(object sender, ElapsedEventArgs e)
        {
            SensorSuhuSeries[0].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 7.14 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f)));
            SensorSuhuSeries[1].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 14.28 + randomizer.NextDouble().Remap(0.0f,1.0f,-2.0f,2.0f)));
            SensorSuhuSeries[2].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 21.42 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f)));
            SensorSuhuSeries[3].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 28.56 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f)));
            SensorSuhuSeries[4].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 35.7 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f)));
            SensorSuhuSeries[5].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), 42.84 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f)));

            CurrTimeAnnotation.X = Axis.ToDouble(DateTime.Now);
            CurrTimeAnnotation.ToolTip = DateTime.Now.ToString(HMSFormat);

            //if ((ChartModel.Annotations[0] as LineAnnotation).X > StartRecordDate + TimeSpan.)

            //ChartModel.Axes[1].Pan(-60.0);
            ChartModel.InvalidatePlot(false);
        }

        public void AddDataSuhu(int sensor_index, double suhu)
        {
            SensorSuhuSeries[sensor_index].Points.Add(new DataPoint(Axis.ToDouble(DateTime.Now), suhu));
            ChartModel.InvalidatePlot(false);
        }

        private void DigitalClockRefresh(object sender, ElapsedEventArgs e)
        {
            var conn = "";
            if (MainWindow.ConnectionController.IsConnected)
            {
                conn = "CONNECTED";
                ChartModel.SubtitleColor = OxyColors.Green;
            }
            else
            {
                conn = "DISCONNECTED";
                ChartModel.SubtitleColor = OxyColors.Red;
            }


            ChartModel.Subtitle = DateTime.Now.ToString($"{conn} - {HMSFormat} - dd / MMMM / yyyy",CultureInfo.GetCultureInfo("id-ID"));
            ChartModel.InvalidatePlot(false);
        }
    }
}
