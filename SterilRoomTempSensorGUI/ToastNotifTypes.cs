using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;
using OxyPlot.Series;

namespace SterilRoomTempSensorGUI
{
    static class NotificationToasts
    {
        public static ToastContentBuilder DisconnectedToast = new ToastContentBuilder()
            .AddHeader("conn", "Device Connection", "reason=disconnected")
            .AddAppLogoOverride(new Uri(Environment.CurrentDirectory + @"\Resources\disconnected.png"))
            .AddText("Koneksi dengan alat Terputus!")
            .AddText("Koneksi aplikasi dengan alat terputus, periksa koneksi Access Point!");

        public static ToastContentBuilder ConnectFailed = new ToastContentBuilder()
            .AddHeader("conn", "Device Connection", "reason=connectfailed")
            .AddAppLogoOverride(new Uri(Environment.CurrentDirectory + @"\Resources\disconnected.png"))
            .AddText("Tidak bisa terkoneksi dengan alat!")
            .AddText("Aplikasi tidak bisa terkoneksi dengan alat, periksa koneksi Access Point!");

    }
}