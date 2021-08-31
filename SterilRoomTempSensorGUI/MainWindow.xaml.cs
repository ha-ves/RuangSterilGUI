using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
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
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using OxyPlot;
using OxyPlot.Series;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using System.Data.SQLite;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Core;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace SterilRoomTempSensorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        const string HMSFormat = "HH\\:mm\\:ss";

        string _currAppStatus = "DISCONNECTED - " + DateTime.Now.ToString($"{HMSFormat} - dd / MMMM / yyyy", CultureInfo.GetCultureInfo("id-ID"));
        public string CurrAppStatus { get { return _currAppStatus; } private set { _currAppStatus = value; OnPropertyChanged("CurrAppStatus"); } }

        bool _connOverlayVisibility = true;
        public bool ConnOverlayVisibility { get { return _connOverlayVisibility; } private set { _connOverlayVisibility = value; OnPropertyChanged("ConnOverlayVisibility"); } }

        static int _overlayBlurRadius = 10;
        public int OverlayBlurRadius { get { return _overlayBlurRadius; } private set { _overlayBlurRadius = value; OnPropertyChanged("OverlayBlurRadius"); } }

        DateTime _startTime;
        public DateTime StartTime { get { return _startTime; } set { _startTime = value; OnPropertyChanged("StartTime"); } }
        DateTime _endTime;
        public DateTime EndTime { get { return _endTime; } set { _endTime = value; OnPropertyChanged("EndTime"); } }

        public List<DataSuhu> dataList { get; private set; }

        public static DataConnectionProcess DataConnController { get; private set; }
        public static SqliteDbAccess LocalDatabase { get; private set; }

        public Notifier[] notifier = new Notifier[6];

        [DllImport("user32.dll")]
        public static extern int FlashWindow(IntPtr Hwnd, bool Revert);

        public static MainWindow Current { get; private set; }

        Timer digitalClockTimer = new Timer(500);
        public static Timer dummyTimer = new Timer(2000);
        public MainWindow()
        {
#if DEBUG
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
#endif
            Current = this;
            DataContext = this;
            System.Threading.Thread.CurrentThread.Name = "MAIN UI THREAD";

            PrepareNotifier();
            _startTime = DateTime.Today;
            _endTime = DateTime.Today + TimeSpan.FromDays(1);

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void PrepareNotifier()
        {
            notifier[0] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopLeft,
                    offsetX: 10,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
            notifier[1] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopLeft,
                    offsetX: 200 + 20,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
            notifier[2] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopLeft,
                    offsetX: 400 + 30,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });

            notifier[3] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 400 + 30,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
            notifier[4] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 200 + 20,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
            notifier[5] = new Notifier(cfg =>
            {
                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;

                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: App.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 30);

                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.FromCount(0));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                Title += " v" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            StartLoading();

            digitalClockTimer.Elapsed += DigitalClockRefresh;
            digitalClockTimer.Start();
        }

        private static void StartLoading()
        {
            DataConnController = new DataConnectionProcess();

            LocalDatabase = new SqliteDbAccess();

            DataConnController.StartConnectAsync();
        }

        public async Task AppStartNormal()
        {
            ToggleOverlay(false);

#if DEBUG
            //await Task.Delay(2000);
            //dummyTimer.Elapsed += AddData;
            //dummyTimer.Start();
#endif
        }

#if DEBUG
        Random randomizer = new Random(67676767);
        private void AddData(object sender, ElapsedEventArgs e)
        {
            double value = 0.0;
            List<double> suhus = new List<double>(6) { 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 5; i++)
            {
                value = /*(i + 1) * 7.14*/ 50.0 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f);
                suhus[i] = value;
                PlotViewModel.AddDataSuhu(i, value);
            }
            value = 46.0 + randomizer.NextDouble().Remap(0.0f, 1.0f, -2.0f, 2.0f);
            suhus[5] = value;
            PlotViewModel.AddDataSuhu(5, value);

            SqliteDbAccess.Current.SaveDataSuhu(new DataSuhu(suhus, DateTime.Now));
        }
#endif

        public static void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            Debug.WriteLine("TOAST ONACTIVATED");
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Obtain any user input (text boxes, menu selections) from the notification
            ValueSet userInput = toastArgs.UserInput;

            // Need to dispatch to UI thread if performing UI operations
            App.Current.Dispatcher.Invoke(() =>
            {
                // TODO: Show the corresponding content
                MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
            });
        }

        private void DigitalClockRefresh(object sender, ElapsedEventArgs e)
        {
            if (DataConnController.IsConnected)
            {
                CurrAppStatus = "CONNECTED - " + DateTime.Now.ToString($"{HMSFormat} - dd / MMMM / yyyy", CultureInfo.GetCultureInfo("id-ID"));
                Dispatcher.Invoke(() => lbl_status.Foreground = Brushes.Green);
            }
            else
            {
                CurrAppStatus = "DISCONNECTED - " + DateTime.Now.ToString($"{HMSFormat} - dd / MMMM / yyyy", CultureInfo.GetCultureInfo("id-ID"));
                Dispatcher.Invoke(() => lbl_status.Foreground = Brushes.Red);
            }
        }

        public void ToggleOverlay(bool visible)
        {
            ConnOverlayVisibility = visible;
            OverlayBlurRadius = visible ? 10 : 0;
        }

        private void PopulateDataTable(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("StartTime : " + StartTime.ToString("G"));
            Debug.WriteLine("EndTime : " + EndTime.ToString("G"));

            dataList = new List<DataSuhu>(SqliteDbAccess.Current.LoadDatalistSuhu(StartTime, EndTime).OrderByDescending(it => it.Waktu));
            OnPropertyChanged("dataList");
        }
    }
}
