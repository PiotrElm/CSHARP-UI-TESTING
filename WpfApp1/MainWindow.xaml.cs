using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace WpfApp1
{
   
    public partial class MainWindow : Window
    {
        String[] ports;
        SerialPort port;
        bool isConnected = false;
        private int myTimer;
        public string response = "";
        public MainWindow()
        {
            InitializeComponent();
            getAllPorts();

            foreach(string port in ports)
            {
                comboBox1.Items.Add(port);
                    if (ports[0] != null)
                {
                    comboBox1.SelectedItem = ports[0];
                }
            }
            void getAllPorts()
            {
                ports = SerialPort.GetPortNames();
            }

            
        }
        private void connect()
        {
            
            string selectedPort = comboBox1.SelectedValue.ToString();
            port = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
            port.Open();
            port.Write("Connection opened!");
            conDisconButton.Content = "Disconnect";
            isConnected = true;
        }

        private void disconnect()
        {
            port.Close();
            conDisconButton.Content = "Connect";
            isConnected = false;
        }

        private void getLine()
        {
             string temp = port.ReadExisting();

            if (isConnected && temp!=null && temp!="")
            {
                response = temp;
            }
            else
            {
            }
        }
        
        private void conDisconButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (!isConnected)
            { 
                connect();
                myTimer = 0;
            }
            else
            {
                disconnect();
                myTimer = 0;
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(500);
            dt.Tick += dtTicker;
            dt.Start();
        }

        private void dtTicker(object sender, EventArgs e)
        {
            if (isConnected)
            {
                responseBlock.Text = "Connected!";
                myTimer++;
                 getLine();
               
            }
            else
            {
                responseBlock.Text = "No connection";
            }
            time_ticks.Content = myTimer/2 + "s";

        }
    }
}
