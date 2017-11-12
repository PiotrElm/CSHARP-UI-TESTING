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
using System.Windows.Threading;

namespace GrblEngineerProject
{
    public class CNCConnection
    {
        Queue SentLinesQueue = Queue.Synchronized(new Queue());
        Queue ManualCommandsQueue = Queue.Synchronized(new Queue());
        public event Action<string> LineReceived;
        public event Action<string> PositionReceived;
        public event Action<string> LineSent;
        private Thread CNCThread;
        private Stream Connection;


        public int ConnectionBaudRate;
        public SerialPort port;
        public bool isConnected;
        public string type = "idle";
        public bool isConfigured = false;
        public string PortName;
        private int _filePosition = 0;
        int StatusPollInterval = 100;
        int ControllerBufferSize = 120;
        int BufferState = 0;
        StreamReader portReader;
        StreamWriter portWriter;

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

            portReader = new StreamReader(Connection);
            portWriter = new StreamWriter(Connection);
            portWriter.Write("\r\n$\n");
            portWriter.Flush();
            this.Work();
        }

        public void disconnect()
        {
            try {
            Connection.Close();
            Connection.Dispose();
            Connection = null;
            isConnected = false;
            CNCThread.Abort();
            }
            catch(Exception e)
            {
                Console.Write(e);
            }
            File = null;
            FilePosition = 0;
            this.type = "idle";
        }
        public void resetZero()
        {
            if(this.isConnected == true && GlobalVariables.MachineStatus =="Idle")
            {
                ManualCommandsQueue.Enqueue("G90 G10 L20 P0 X0 Y0 Z0");
            }
        }
        public void resumeMachine()
        {
            if (this.isConnected == true)
            {
                ManualCommandsQueue.Enqueue("~");
            }
        }
        public void holdMachine()
        {
            if (this.isConnected == true )
            {
                ManualCommandsQueue.Enqueue("!");
            }
        }
        public void manualCommand(string command)
        {
            if (this.isConnected == true && GlobalVariables.MachineStatus == "Idle")
            {
                ManualCommandsQueue.Enqueue(command);
            }
        }
        public void unlockMachine()
        {
            if (this.isConnected == true && GlobalVariables.MachineStatus == "Idle")
            {
                ManualCommandsQueue.Enqueue("$X");
            }
        }
        public void Work()
        {
            CNCThread = new Thread(CNCWorks);
            CNCThread.Priority = ThreadPriority.AboveNormal;
            CNCThread.Start();
        }

        private void CNCWorks()
        {
                try
                {
                DateTime LastStatusPoll = DateTime.Now + TimeSpan.FromSeconds(0.5);
                DateTime StartTime = DateTime.Now;
                while (true)
                    {
                        if (!isConnected)
                        {
                            return;
                        }

                    Task<string> lineTask = portReader.ReadLineAsync();

                    while (!lineTask.IsCompleted)
                    {
                        if (this.type=="file" && File.Count > FilePosition && (File[FilePosition].Length + 1) < (ControllerBufferSize - BufferState))
                        {

                            string send_line = File[FilePosition++];
                            portWriter.Write(send_line);
                            portWriter.Write('\n');
                            portWriter.Flush();
                            ActionStart(LineSent, send_line);
                            SentLinesQueue.Enqueue(send_line);
                            BufferState += send_line.Length + 1;
                            continue;
                            }
                        else if(ManualCommandsQueue.Count >0)
                        {
                            string send_line = ManualCommandsQueue.Dequeue().ToString();
                            portWriter.Write(send_line);
                            portWriter.Write('\n');
                            portWriter.Flush();
                            SentLinesQueue.Enqueue(send_line);
                            ActionStart(LineSent, send_line);
                            BufferState += send_line.Length + 1;
                        }

                        DateTime Now = DateTime.Now;

                        if ((Now - LastStatusPoll).TotalMilliseconds > StatusPollInterval)
                        {
                            portWriter.Write('?');
                            portWriter.Flush();
                            LastStatusPoll = Now;
                        }

                    }

                        string line = lineTask.Result;

                
                    if (line.StartsWith("<"))
                    {
                        ActionStart(PositionReceived, line);          
                    }
                    else
                    {
                        ActionStart(LineReceived, line);
                       
                        if (SentLinesQueue.Count != 0)
                        {
                            BufferState -= ((string)SentLinesQueue.Dequeue()).Length + 1;
                        }

                    }

                    }

                }
                catch (Exception workexception)
                {
                MessageBox.Show(workexception.ToString());
                    //Console.WriteLine(workexception.ToString());
                    disconnect();
                }
        }


        public void LoadFile(IList<string> file)
        {
            File = new ReadOnlyCollection<string>(file);
            FilePosition = 0;
        }

        private void ActionStart(Action<string> action, string param)
        {
            if (action == null)
                return;
            App.Current.Dispatcher.BeginInvoke(action, param);
        }
    }
}
