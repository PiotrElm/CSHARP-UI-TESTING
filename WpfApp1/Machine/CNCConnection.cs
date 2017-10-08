using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using GrblEngineerProject;
using System.Collections.ObjectModel;

namespace GrblEngineerProject.Machine
{
    public class CNCConnection
    {
        public event Action<string> LineReceived;
        public event Action<string> LineSent;
        private Stream Connection;
        private Thread WorkerThread;


        public int ConnectionBaudRate;
        public SerialPort port;
        public bool isConnected;
        public bool isConfigured = false;
        public bool isCNCIdle = true;
        public string PortName;
        

        public void saveSettings()
        {
            port = new SerialPort(PortName, ConnectionBaudRate, Parity.None, 8, StopBits.One);
            isConfigured = true;
        }
         public void connect()
        {
            port.Open();
            Connection = port.BaseStream;
            isConnected = true;
            CNCWorks();
        }

        public void disconnect()
        {
            Connection.Close();
            Connection.Dispose();
            Connection = null;
            isConnected = false;
        }


        private void CNCWorks()
        {
            StreamReader portReader = new StreamReader(Connection);
            StreamWriter portWriter = new StreamWriter(Connection);
            portWriter.Write("\n");
            portWriter.Flush();
            Task<string> lineTask = portReader.ReadLineAsync();
            string line = lineTask.Result;
            RaiseEvent(LineReceived, line);
        }

        public void getPosition()
        {
            StreamReader portReader = new StreamReader(Connection);
            StreamWriter portWriter = new StreamWriter(Connection);
            portWriter.Write("?");
            portWriter.Flush();
            Task<string> lineTask = portReader.ReadLineAsync();
            string line = lineTask.Result;
            RaiseEvent(LineReceived, line);
        }
        private void RaiseEvent(Action<string> action, string param)
        {
            if (action == null)
                return;

            App.Current.Dispatcher.BeginInvoke(action, param);
        }
    }
}
