using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;

namespace SterilRoomTempSensorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ConnectionController ConnectionController { get; private set; } = new ConnectionController();

        public MainWindow()
        {
#if DEBUG
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
#endif
            InitializeComponent();

            //Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingIcon.RenderTransform.BeginAnimation(
                RotateTransform.AngleProperty,
                new DoubleAnimation()
                {
                    From = 0.0,
                    To = 360.0,
                    Duration = TimeSpan.FromSeconds(1),
                    RepeatBehavior = RepeatBehavior.Forever
                });
        }
    }
}
