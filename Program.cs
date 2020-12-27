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
            try
            {
                using SerialPort sp = new SerialPort("/dev/serial0");
                sp.Encoding = Encoding.UTF8;
                sp.BaudRate = 9600;
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;
                sp.Open();

                bool finished = false;
                Console.CancelKeyPress += (a, b) =>
                {
                    finished = true;
                    // close port to kill pending operations
                    sp.Close();
                };

                while(!finished)
                {
                    Console.WriteLine("Warte auf Signale...");
                    Thread.Sleep(30000);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Es ist ein Fehler aufgetreten: " + ex.ToString());
            }
        }
    }
}
