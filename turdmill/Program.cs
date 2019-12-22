using System;
using System.IO.Ports;
using System.Linq;

namespace turdmill
{
    class SerialPortProgram
    {
        // Create the serial port with basic settings
        private SerialPort port = new SerialPort("COM6",
            9600, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.RequestToSend
        };

        private static void Main(string[] args)
        {
            new SerialPortProgram();
        }

        // Generates a 1 byte XOR checksum for a byte array.
        private byte checksumFromBytes(byte[] bytes)
        {
            byte checksum = 0x00;

            for (int i = 0; i < bytes.Length; i++)
            {
                byte theByte = bytes[i];
                checksum ^= theByte;
            }

            return checksum;
        }

        private SerialPortProgram()
        {
            // Attach a method to be called when there
            // is data waiting in the port's buffer
            port.DataReceived += new
              SerialDataReceivedEventHandler(HandleDataReceived);

            // Begin communications
            port.Open();

            // 0x00: checksum byte
            var emptyFrame = new byte[] { 0xF1, 0x00, 0xF2 };
            var cmdGetVersion = new byte[] { 0xF1, 0x91, 0x91, 0xF2 };
            var cmdGetStatus = new byte[] { 0xF1, 0x80, 0x80, 0xF2 };
            var cmdGoReady = new byte[] { 0xF1, 0x81, 0x81, 0xF2 };
            var cmdGoIdle = new byte[] { 0xF1, 0x82, checksumFromBytes(new byte[] { 0x82 }), 0xF2 };
            var cmdSetSpeed = new byte[] { 0xF1, 0x25, 0x25, 0xF2 };

            var cmd = cmdGetStatus;

            var cmds = new byte[][] { cmdGoReady, cmdGoIdle };

            foreach (var cmd2 in cmds)
            {
                Console.WriteLine("Sending {0}", BitConverter.ToString(cmd2));
                port.Write(cmd2, 0, cmd2.Length);
                System.Threading.Thread.Sleep(1000);
            }

            int i = 0;
            while (true)
            {
                Console.WriteLine("({0}) Sending {1}", i++, BitConverter.ToString(cmd));
                port.Write(cmd, 0, cmd.Length);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void HandleDataReceived(object sender,
          SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[1024];
            int read = port.Read(buffer, 0, 1024);

            var bytes = buffer.Take(read).ToArray();

            // Show all the incoming data in the port's buffer
            Console.WriteLine("Received data: '{0}'", BitConverter.ToString(bytes));
        }
    }
}
