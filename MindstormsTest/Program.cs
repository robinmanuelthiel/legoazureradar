using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindstormsTest
{
    class Program
    {
        private static Brick _Brick;

        static void Main(string[] args)
        {
            
              
            _Brick = new Brick(new BluetoothCommunication("4"));
            _Brick.BrickChanged += _Brick_BrickChanged;
            _Brick.ConnectAsync();

            Console.ReadLine();
        }        

        private async static void _Brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            await _Brick.DirectCommand.PlayToneAsync(100, 1000, 300);
        }
    }
}
