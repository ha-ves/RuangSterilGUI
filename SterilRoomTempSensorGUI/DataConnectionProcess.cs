using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Threading;

namespace SterilRoomTempSensorGUI
{
    public class DataConnectionProcess
    {
        public bool IsConnected { get; private set; }

        TcpClient TcpClient;
        IPEndPoint deviceEP;
        SocketAsyncEventArgs receivingArgs;

        byte[] recvBuf;
        readonly List<char> recvHeading;

        List<bool> suhuUpdated;
        List<double> suhus;

        public DataConnectionProcess()
        {
            recvHeading = new List<char>() { '@', '$', '%', '&', '*', '!' };
            recvBuf = new byte[8192];
            IsConnected = false;
            suhuUpdated = new List<bool>(6) { false, false, false, false, false, false };
            suhus = new List<double>(6) { 0, 0, 0, 0, 0, 0 };

            receivingArgs = new SocketAsyncEventArgs();
            receivingArgs.Completed += TcpReceiveData;
            receivingArgs.SetBuffer(recvBuf, 0, recvBuf.Length);
        }

        public async Task StartConnectAsync()
        {
            List<NetworkInterface> interfaces = new List<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces().Where(intf => intf.OperationalStatus == OperationalStatus.Up));
            if (interfaces.Count != 0)
            {
                foreach(var interf in interfaces)
                {
                    foreach(var gatewayIP in interf.GetIPProperties().GatewayAddresses)
                    {
                        Debug.WriteLine($"Try Connecting [ {gatewayIP.Address} ]");

                        //deviceEP = new IPEndPoint(IPAddress.Loopback, 12727);
                        deviceEP = new IPEndPoint(gatewayIP.Address, 12727);

                        TcpClient = new TcpClient() { NoDelay = true };

                        if (await Task.WhenAny(TcpClient.Client.ConnectAsync(deviceEP), Task.Delay(1000)) != null && TcpClient.Connected)
                        {
                            MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                            IsConnected = true;
                            receivingArgs.RemoteEndPoint = deviceEP;
                            TcpClient.Client.ReceiveAsync(receivingArgs);
                            MainWindow.Current.AppStartNormal();
                            App.Current.Dispatcher.Invoke(() => MainWindow.Current.ToggleOverlay(false));
                            Debug.WriteLine($"Connected to access point [ {TcpClient.Client.RemoteEndPoint} ]");
                            return;
                        }
                    }
                }
                MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                if(MessageBox.Show("Periksa kesesuaian Access Point alat!\n" +
                    "Koneksikan sistem dengan Access Point alat dan klik [OK].\n\n" +
                    "Jika hanya ingin melihat catatan Data Suhu klik [CANCEL]", "ERROR : Alat tidak ditemukan!", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                    == MessageBoxResult.Cancel)
                    App.Current.Dispatcher.Invoke(() => MainWindow.Current.ToggleOverlay(true, true));
            }
            else
            {
                MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                MessageBox.Show("Platform anda tidak memiliki jaringan (Network). Monitoring tidak bisa dijalankan\n" +
                    "Hanya bisa melihat catatan data suhu.", "ERROR : Tidak ada jaringan!", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Current.Dispatcher.Invoke(() => MainWindow.Current.ToggleOverlay(true, true));
            }
            StartConnectAsync();
        }

        private void TcpReceiveData(object sender, SocketAsyncEventArgs e)
        {
            ParseData(e);
            TcpClient.Client.ReceiveAsync(receivingArgs);
        }

        private async Task ParseData(SocketAsyncEventArgs e, double suhu = 0)
        {
            List<string> datas = Encoding.ASCII.GetString(recvBuf.Take(e.BytesTransferred).ToArray()).TrimEnd('\n', '\r').Split('#').ToList();
            if (string.IsNullOrEmpty(datas.Last())) datas.Remove(datas.Last());

            foreach (var data in datas)
            {
                if (recvHeading.Contains(data[0]) && double.TryParse(data.TrimStart(recvHeading.ToArray()), out suhu))
                {
                    var index = recvHeading.IndexOf(data[0]);
                    suhus[index] = suhu;
                    PlotViewModel.AddDataSuhu(index, suhu);
                    suhuUpdated[index] = true;
                }
                if (suhuUpdated.TrueForAll(it => it == true))
                {
                    suhuUpdated = new List<bool>(6) { false, false, false, false, false, false };
                    SqliteDbAccess.Current.SaveDataSuhu(new DataSuhu(suhus, DateTime.Now));
                    suhus = new List<double>(6) { 0, 0, 0, 0, 0, 0 };
                }
            }
        }
    }
}
