using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MindstormsTest.Wpf
{
    public partial class MainWindow : Window
    {
        private DeviceClient deviceClient;
        private Brick mindstormsBrick;        
        private DispatcherTimer timer;

        private string iotHubUri = "legoiothub.azure-devices.net";
        private string deviceId = "LEGOEV3";
        private string deviceKey = "G35+JOKSUHYXcxZ8DD6VEwsbdUHAAXg5Ld48lYzmTBM=";
        private int Volume = 100;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;

            try
            {
                Log("Establishing Azure connection...", false);
                deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
                Log(" success!");
            }
            catch (Exception)
            {
                Log(" failed!");
            }

            mindstormsBrick = new Brick(new UsbCommunication());
            mindstormsBrick.BrickChanged += MindstormsBrick_BrickChanged;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {            
            try
            {
                Log("Connecting with EV3 Brick via USB...", false);
                await mindstormsBrick.ConnectAsync();
                Log(" success!");
            }
            catch (Exception)
            {
                Log("EV3 Brick connection failed :(");
                Log(" failed!");
            }
        }

        private void MindstormsBrick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var distance = mindstormsBrick.Ports[InputPort.One].SIValue;
            Log($"{now} - Distance: {distance.ToString()}");
            
            if (distance < 255)
            {
                // Play near sound  
                await mindstormsBrick.DirectCommand.PlayToneAsync(100, 1000, 300);
            }       
            else
            {
                // Play far sound
                await mindstormsBrick.DirectCommand.PlayToneAsync(Volume, 200, 100);
            }

            // Format data to a JSON message
            var telemetryDataPoint = new { id = deviceId, distance = distance, dateTime = now };
            var json = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(json));

            // Send message to the IoT Hub
            await deviceClient.SendEventAsync(message);
        }

        private void Log(string text, bool lineBreak = true)
        {
            LogText.Text += (text);
            if (lineBreak)
                LogText.Text += "\n";

            LogText.ScrollToEnd();
        }
    }
}