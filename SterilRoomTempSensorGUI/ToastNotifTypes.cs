using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;
using OxyPlot.Series;

namespace SterilRoomTempSensorGUI
{
    static class NotificationToasts
    {
        public static ToastContentBuilder DisconnectedToast = new ToastContentBuilder()
            .AddAppLogoOverride(new Uri(Environment.CurrentDirectory + @"\Resources\disconnected.png"), ToastGenericAppLogoCrop.None)
            .AddText("Koneksi dengan alat Terputus!")
            .AddText("Koneksi aplikasi dengan alat terputus, periksa koneksi Access Point!");
    }
}