using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Messages;

namespace SterilRoomTempSensorGUI
{
    public static class ExtensionMethods
    {
        public static double Remap(this double value, float from_min, float from_max, float to_min, float to_max)
        {
            return (value - from_min) / (from_max - from_min) * (to_max - to_min) + to_min;
        }
    }

    class PlotViewModel
    {
        const string HMSFormat = "HH\\:mm\\:ss";
        const string SuhuFormat = "0.##°C";
        static DateTime StartRecordDate { get; set; } = DateTime.Now;

        public static List<PlotModel> ChartViews { get; private set; } = new List<PlotModel>();
        static List<DateTimeAxis> TimeAxes { get; set; } = new List<DateTimeAxis>();
        static List<LinearAxis> SuhuAxes { get; set; } = new List<LinearAxis>();
        static List<LineAnnotation> CurrTimeAnnotations { get; set; } = new List<LineAnnotation>(); 
        static List<LineSeries> SensorSuhuSeries { get; set; } = new List<LineSeries>();
        static bool[] IsAbnormal = new bool[6] { false, false, false, false, false, false };

        public PlotViewModel()
        {
            for (int i = 0; i < 6; i++)
            {
                SuhuAxes.Add(new LinearAxis()
                {
                    Title = "Suhu (Celcius)",
                    StringFormat = SuhuFormat,
                    Position = AxisPosition.Left,
                    AbsoluteMinimum = 0,
                    AbsoluteMaximum = 100,
                    MinimumRange = 100,
                    IsZoomEnabled = false,
                    MajorStep = 10,
                    MinorStep = 2,
                    MajorGridlineStyle = LineStyle.Automatic,
                    MajorGridlineThickness = 1.5,
                    MajorGridlineColor = OxyColors.LightGray,
                    MinorGridlineStyle = LineStyle.Automatic,
                    MinorGridlineThickness = 0.5,
                    MinorGridlineColor = OxyColors.LightGray
                });

                TimeAxes.Add(new DateTimeAxis()
                {
                    Title = "Waktu Sistem",
                    StringFormat = HMSFormat,
                    Position = AxisPosition.Bottom,
                    IntervalType = DateTimeIntervalType.Seconds,
                    AbsoluteMinimum = Axis.ToDouble(StartRecordDate),
                    MinimumRange = 0.00005,
                    MaximumRange = 1,
                    MajorGridlineStyle = LineStyle.Automatic,
                    MajorGridlineThickness = 1.5,
                    MajorGridlineColor = OxyColors.LightGray
                });
                TimeAxes[i].Zoom(Axis.ToDouble(StartRecordDate), Axis.ToDouble(StartRecordDate + TimeSpan.FromMinutes(1)));

                SensorSuhuSeries.Add(new LineSeries()
                {
                    Title = $"Sensor {i + 1}",
                    TrackerFormatString = "Data {0}\n" +
                                            "Suhu : {4:" + SuhuFormat + "}\n" +
                                            "Pukul {2:" + HMSFormat + "}\n",
                    RenderInLegend = false
                });

                CurrTimeAnnotations.Add(new LineAnnotation()
                {
                    Type = LineAnnotationType.Vertical,
                    X = Axis.ToDouble(StartRecordDate)
                });

                ChartViews.Add(new PlotModel()
                {
                    Title = $"Sensor {i + 1}",
                    Subtitle = "Update Terakhir : ",
                    DefaultFontSize = 14,
                    SubtitleFontWeight = 500,
                    PlotAreaBorderColor = OxyColors.Gray,
                    SubtitleColor = OxyColors.DarkGreen
                });

                ChartViews[i].Axes.Add(SuhuAxes[i]);
                ChartViews[i].Axes.Add(TimeAxes[i]);
                ChartViews[i].Series.Add(SensorSuhuSeries[i]);
                ChartViews[i].Annotations.Add(CurrTimeAnnotations[i]);
            }
        }

        public static void AddDataSuhu(int sensor_index, double suhu)
        {
            //if (sensor_index < 0) return;
            var timeUsed = DateTime.Now;

            if (suhu < 45 || suhu > 55)
            {
                if (!IsAbnormal[sensor_index])
                {
                    IsAbnormal[sensor_index] = true;
                    ChartViews[sensor_index].Background = OxyColors.Yellow;
                    var option = new ToastNotifications.Core.MessageOptions() { FontSize = 14 };
                    App.Current.Dispatcher.Invoke(() => MainWindow.Current.notifier[sensor_index].ShowWarning($"SENSOR {sensor_index + 1} ABNORMAL", option));
                }
            }
            else
            {
                if (IsAbnormal[sensor_index])
                {
                    IsAbnormal[sensor_index] = false;
                    ChartViews[sensor_index].Background = OxyColors.Undefined;
                    App.Current.Dispatcher.Invoke(() => MainWindow.Current.notifier[sensor_index].ClearMessages(new ClearAll()));
                }
            }

            SensorSuhuSeries[sensor_index].Points.Add(new DataPoint(Axis.ToDouble(timeUsed), suhu));

            CurrTimeAnnotations[sensor_index].X = Axis.ToDouble(timeUsed);
            CurrTimeAnnotations[sensor_index].ToolTip = timeUsed.ToString(HMSFormat);

            if (CurrTimeAnnotations[sensor_index].X > TimeAxes[sensor_index].ActualMaximum)
            {
                //Debug.WriteLine("GETTING OUT OF BOUND");

                //and you want to pan only the time axis of your plot (in this example the x-Axis).
                double firstValue = timeUsed.ToOADate() - DateTimeAxis.ToDateTime(CurrTimeAnnotations[sensor_index].X - TimeAxes[sensor_index].ActualMaximum).ToOADate();
                double secondValue = timeUsed.ToOADate();
                
                //Debug.WriteLine($"SeconValue : {secondValue}");
                //Transfrom the x-Values (DateTime-Value in OLE Automation format) to screen-coordinates
                double transformedfirstValue = TimeAxes[sensor_index].Transform(firstValue) - 1;
                double transformedsecondValue = TimeAxes[sensor_index].Transform(secondValue);

                //the pan method will calculate the screen coordinate difference/distance and will pan you axsis based on this amount
                //if you are planing on panning your y-Axis or both at the same time, you  will need to create different ScreenPoints accordingly
                TimeAxes[sensor_index].Pan(
                  new ScreenPoint(transformedsecondValue, 0),
                  new ScreenPoint(transformedfirstValue, 0)
                );
            }

            ChartViews[sensor_index].Subtitle = $"Update Terakhir : [ {timeUsed.ToString(HMSFormat)} | {suhu.ToString(SuhuFormat)} ]";
            ChartViews[sensor_index].InvalidatePlot(false);
        }
    }
}
