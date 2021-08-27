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

        public DataConnectionProcess()
        {
            recvHeading = new List<char>() { '@', '$', '%', '&', '*', '!' };
            recvBuf = new byte[8192];
            IsConnected = false;

            receivingArgs = new SocketAsyncEventArgs();
            receivingArgs.Completed += TcpReceiveData;
            receivingArgs.SetBuffer(recvBuf, 0, recvBuf.Length);
        }

        private void TcpReceiveData(object sender, SocketAsyncEventArgs e)
        {
            TcpClient.Client.ReceiveAsync(receivingArgs);
            
            Debug.WriteLine($"New data from [ {e.RemoteEndPoint} ]");
            Debug.WriteLine("Data :\n");
            for (int i = 0; i < e.BytesTransferred; i++)
            {
                Debug.Write(e.Buffer[i].ToString("{0:X2}") + ' ');
            }
            Debug.WriteLine("");

            ParseData(e);
        }

        private void ParseData(SocketAsyncEventArgs e, double suhu = 0)
        {
            var data = Encoding.ASCII.GetString(recvBuf.Take(e.BytesTransferred).ToArray()).TrimEnd('\n', '\r');
            Debug.WriteLine($"Data : {data}");

            if (!recvHeading.Contains((char)data[0])
                || !data[data.Length - 1].Equals('#')
                || !double.TryParse(data.TrimStart(recvHeading.ToArray()).TrimEnd('#'), out suhu))
            {
                Debug.WriteLine("Data is not valid.");
                return;
            }

            PlotViewModel.AddDataSuhu(recvHeading.IndexOf((char)data[0]), suhu);
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
                        deviceEP = new IPEndPoint(gatewayIP.Address, 12727);
                        //deviceEP = new IPEndPoint(IPAddress.Loopback, 12727);

                        TcpClient = new TcpClient() { NoDelay = true };
                        try
                        {
                            if(await Task.WhenAny(TcpClient.Client.ConnectAsync(deviceEP), Task.Delay(1000)) != null && TcpClient.Connected)
                            {
                                IsConnected = true;
                                receivingArgs.RemoteEndPoint = deviceEP;
                                TcpClient.Client.ReceiveAsync(receivingArgs);
                                MainWindow.Current.AppStartNormal();
                                MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                                Debug.WriteLine($"Connected to access point [ {TcpClient.Client.RemoteEndPoint} ]");
                                return;
                            }
                        }
                        catch (Exception Exc)
                        {
                            Debug.WriteLine($"[StartConnectAsync] { Exc.Message }");
                            return;
                        }
                    }
                }
                MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                MessageBox.Show("Periksa kesesuaian Access Point alat!\nKoneksikan sistem dengan Access Point alat dan klik [OK].", "ERROR : Alat tidak ditemukan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MainWindow.FlashWindow(new WindowInteropHelper(MainWindow.Current).Handle, false);
                MessageBox.Show("Platform anda tidak memiliki jaringan (Network). Aplikasi tidak bisa dijalankan.", "ERROR : Tidak ada jaringan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            StartConnectAsync();
        }
    }
}
