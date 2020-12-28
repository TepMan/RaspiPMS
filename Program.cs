using System;
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

            int FRAME_SIZE = 32;
            byte START_BYTE_1 = 0x42;
            byte START_BYTE_2 = 0x4D;
            byte[] SLEEP_CMD_BYTES = { START_BYTE_1, START_BYTE_2, (byte) 0xE4, (byte) 0x00, (byte) 0x00, (byte) 0x01, (byte) 0x73 };
            byte[] WAKEUP_CMD_BYTES = { START_BYTE_1, START_BYTE_2, (byte) 0xE4, (byte) 0x00, (byte) 0x01, (byte) 0x01, (byte) 0x74 };

            try
            {
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
                    Console.WriteLine("Program canceled by user.");
                };

                while(!finished)
                {
                    Console.WriteLine("Try to wake up sensor...");
                    sp.Write(WAKEUP_CMD_BYTES, 0, WAKEUP_CMD_BYTES.Length);
                    Thread.Sleep(30000);
                    
                    Console.WriteLine("Try to read data...");

                    byte[] response = new byte[FRAME_SIZE];

                    sp.Read(response, 0, FRAME_SIZE);

                    Console.WriteLine("Rawdata: " + ConvertToHexString(response));

                    Console.WriteLine("CF1 values:");
                    Console.WriteLine("PM 1.0: " + ConvertBytesToValue(response, 4));
                    Console.WriteLine("PM 2.5: " + ConvertBytesToValue(response, 6));
                    Console.WriteLine("PM 10 : " + ConvertBytesToValue(response, 8));

                    Console.WriteLine("Atmospheric values:");
                    Console.WriteLine("PM 1.0: " + ConvertBytesToValue(response, 10));
                    Console.WriteLine("PM 2.5: " + ConvertBytesToValue(response, 12));
                    Console.WriteLine("PM 10 : " + ConvertBytesToValue(response, 14));

                    Console.WriteLine("Particles / 0.1L of air values:");
                    Console.WriteLine("PM 0.3: " + ConvertBytesToValue(response, 16));
                    Console.WriteLine("PM 0.5: " + ConvertBytesToValue(response, 18));
                    Console.WriteLine("PM 1.0: " + ConvertBytesToValue(response, 20));
                    Console.WriteLine("PM 2.5: " + ConvertBytesToValue(response, 22));
                    Console.WriteLine("PM 5.0: " + ConvertBytesToValue(response, 24));
                    Console.WriteLine("PM 10 : " + ConvertBytesToValue(response, 26));
                    
                    Console.WriteLine("Send seonsor to sleep mode...");
                    sp.Write(SLEEP_CMD_BYTES, 0, SLEEP_CMD_BYTES.Length);
                    Thread.Sleep(30000);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured: " + ex.ToString());
            }
        }

        private static string ConvertToHexString(byte[] bytes) 
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);

            foreach(byte b in bytes)
            {
                builder.Append(b.ToString("X2") + " ");
            }

            return builder.ToString();
	    }

        private static uint ConvertBytesToValue(byte[] bytes, int index) 
        {
		    return (Convert.ToUInt32(bytes[index]) << 8) + Convert.ToUInt32(bytes[index + 1]);
	    }
    }
}
