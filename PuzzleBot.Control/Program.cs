using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuzzleBotSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new TcpClient("192.168.1.74", 2000);
            var s = c.GetStream();

            var thread1 = new Action(() => {
                while (true) {
                    var b = s.ReadByte();
                    Console.Write((char)b);
                }
            });

            var thread2 = new Action(() => {
                while (true) {
                    var k = Console.ReadKey();
                    s.WriteByte((byte)k.KeyChar);
                }
            });

            new Task(thread1).Start();
            new Task(thread2).Start();
            new ManualResetEvent(false).WaitOne();
        }
    }



    public interface IChannel
    {
        void Send(byte[] data);
    }


}
