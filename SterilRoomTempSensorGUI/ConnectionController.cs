using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SterilRoomTempSensorGUI
{
    public class ConnectionController
    {
        public bool IsConnected { get; private set; } = false;
        TcpClient TcpClient;
        public bool ConnOverlayVisible { get; private set; } = false;

        public ConnectionController()
        {
            TcpClient = new TcpClient() { NoDelay = true };
        }

        //public event 

        public async Task<ConnectionController> StartConnectAsync()
        {
            List<NetworkInterface> interfaces = new List<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces().Where(intf => intf.OperationalStatus == OperationalStatus.Up));
            if (interfaces.Count != 0)
            {
                interfaces.ForEach((interf) =>
                {
                    interf.GetIPProperties().GatewayAddresses.ToList().ForEach(async (gatewayIP) =>
                    {
                        await TcpClient.ConnectAsync(gatewayIP.Address, 12727);
                        //if (TcpClient.Connected) 
                    });
                });
            } else MessageBox.Show("Tidak ada jaringan!", "Sistem tidak tersambung ke jaringan. Periksa koneksi ke Access Point!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return this;
        }
    }
}
