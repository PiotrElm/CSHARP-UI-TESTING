using System;
using System.Windows;
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
        private int _filePosition = 0;


        private ReadOnlyCollection<string> _file = new ReadOnlyCollection<string>(new string[0]);
        public ReadOnlyCollection<string> File
        {
            get { return _file; }
            set
            {
                _file = value;
                FilePosition = 0;
            }
        }
        public int FilePosition
        {
            get { return _filePosition; }
            private set
            {
                _filePosition = value;
            }
        }

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
        }

        public void disconnect()
        {
            try {
            Connection.Close();
            Connection.Dispose();
            Connection = null;
            isConnected = false;
                isCNCIdle = true;
            }
            catch(Exception e)
            {
            }
        }
        public void Work()
        {
            WorkerThread = new Thread(CNCWorks);
            WorkerThread.Priority = ThreadPriority.AboveNormal;
            WorkerThread.Start();
        }

        private void CNCWorks()
        {
           
                try
                {
                    StreamReader portReader = new StreamReader(Connection);
                    StreamWriter portWriter = new StreamWriter(Connection);
                    portWriter.Write("\n$G\n");
                    portWriter.Flush();
                    while (true)
                    {
                        if (!isConnected)
                        {
                            return;
                        }

                        Task<string> lineTask = portReader.ReadLineAsync();
                    while (!lineTask.IsCompleted)
                    {
                        if (File.Count > FilePosition)
                        {
                            isCNCIdle = false;
                            string send_line = File[FilePosition++];
                            portWriter.Write(send_line);
                            portWriter.Write('\n');
                            portWriter.Flush();
                            RaiseEvent(LineSent, send_line);
                            while (lineTask.Result == "") {}
                            continue;
                            }
                        
                        }
                        string line = lineTask.Result;
                        RaiseEvent(LineReceived, line);
                  
                    }

                }
                catch (Exception workexception)
                {
                    Console.WriteLine(workexception);
                    disconnect();
                }
            
           
            
        }

        public void LoadFile(IList<string> file)
        {
            File = new ReadOnlyCollection<string>(file);
            FilePosition = 0;
        }

        private void RaiseEvent(Action<string> action, string param)
        {
            if (action == null)
                return;

            App.Current.Dispatcher.BeginInvoke(action, param);
        }
    }
}
