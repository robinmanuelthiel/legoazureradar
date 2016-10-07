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

        //private string iotHubUri = "legoiothub.azure-devices.net";
        //private string deviceId = "LEGOEV3";
        //private string deviceKey = "G35+JOKSUHYXcxZ8DD6VEwsbdUHAAXg5Ld48lYzmTBM=";
        private int Volume = 100;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
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
            var telemetryDataPoint = new { id = tbkDeviceId.Text, distance = distance, dateTime = now };
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

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = (int)e.NewValue;
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            bool running = true;
            tbkIotHubUrl.IsEnabled = false;
            tbkDeviceId.IsEnabled = false;
            tbkDeviceKey.IsEnabled = false;

            try
            {
                Log("Establishing Azure connection...", false);
                deviceClient = DeviceClient.Create(tbkIotHubUrl.Text, new DeviceAuthenticationWithRegistrySymmetricKey(tbkDeviceId.Text, tbkDeviceKey.Text), TransportType.Mqtt);
                Log(" success!\n");
            }
            catch (Exception)
            {
                Log(" failed!");
                Log("Please check the Azure IoT Hub URL, Device ID or Device Key.\n");
                running = false;
            }

            try
            {
                Log("Connecting with EV3 Brick via USB...", false);
                mindstormsBrick = new Brick(new UsbCommunication());
                mindstormsBrick.BrickChanged += MindstormsBrick_BrickChanged;
                await mindstormsBrick.ConnectAsync();
                Log(" success!");
            }
            catch (Exception)
            {
                Log(" failed!");
                Log("Please check the USB connection and switch the EV3 on.\n");
                running = false;
            }

            if (!running)
            {
                tbkIotHubUrl.IsEnabled = true;
                tbkDeviceId.IsEnabled = true;
                tbkDeviceKey.IsEnabled = true;
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            timer.Stop();
            tbkIotHubUrl.IsEnabled = true;
            tbkDeviceId.IsEnabled = true;
            tbkDeviceKey.IsEnabled = true;
        }
    }
}