using Lego.Ev3.Core;
using Lego.Ev3.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MindstormsTest.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Brick MindstormsBrick;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log("Program started.");
            Log("Connect with EV3 Brick...");            

            MindstormsBrick = new Brick(new Lego.Ev3.UWP.BluetoothCommunication());
            MindstormsBrick.BrickChanged += MindstormsBrick_BrickChanged;
            await MindstormsBrick.ConnectAsync();

        }

        private void MindstormsBrick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            Log("EV3 Brick connected.");
        }

        private void Log(string text)
        {
            LogText.Text += (text + "\n");
        }
    }
}
