using System;
using System.Device.Gpio;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace RaspiPMS
{
    class Program
    {
        static void Main(string[] args)
        {
            bool finished = false;

            int pin = 18;

            int FRAME_SIZE = 32;
            byte START_BYTE_1 = 0x42;
            byte START_BYTE_2 = 0x4D;
            byte[] SLEEP_CMD_BYTES = { START_BYTE_1, START_BYTE_2, (byte) 0xE4, (byte) 0x00, (byte) 0x00, (byte) 0x01, (byte) 0x73 };
            byte[] WAKEUP_CMD_BYTES = { START_BYTE_1, START_BYTE_2, (byte) 0xE4, (byte) 0x00, (byte) 0x01, (byte) 0x01, (byte) 0x74 };

            try
            {
                using GpioController controller = new();
                controller.OpenPin(pin, PinMode.Output);

                using SerialPort sp = new SerialPort("/dev/serial0");
                sp.Encoding = Encoding.UTF8;
                sp.BaudRate = 9600;
                sp.DataBits = 8;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
                sp.ReadTimeout = 10000;
                sp.WriteTimeout = 1000;
                sp.Open();

                Console.CancelKeyPress += (a, b) =>
                {
                    finished = true;
                    // close port to kill pending operations
                    sp.Close();
                    Console.WriteLine("Programm durch Anwender beendet.");
                };

                while(!finished)
                {
                    Console.WriteLine("Versuche, den Sensor zu wecken...");
                    controller.Write(pin, PinValue.High);
                    sp.Write(WAKEUP_CMD_BYTES, 0, WAKEUP_CMD_BYTES.Length);
                    Thread.Sleep(30000);
                    
                    Console.WriteLine("Versuche, Daten zu lesen...");

                    byte[] response = new byte[FRAME_SIZE];

                    for(int idx = 0; idx < FRAME_SIZE; idx++)
                    {
                        response[idx] = (byte)sp.ReadByte();
                    }

                    Console.WriteLine("Gelesene Daten: " + ConvertToHexString(response));
                    
                    Console.WriteLine("Versuche, den Sensor schlafen zu legen...");
                    sp.Write(SLEEP_CMD_BYTES, 0, SLEEP_CMD_BYTES.Length);
                    controller.Write(pin, PinValue.Low);
                    Thread.Sleep(30000);
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("Es ist ein Fehler aufgetreten: " + ex.ToString());
            }
        }

        private static string ConvertToHexString(byte[] bytes) 
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);

            foreach(byte b in bytes)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
	    }
    }
}
