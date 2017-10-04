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
        int selectedBaudrate;
        bool isConnected = false;
        private int myTimer;
        public string response = "";
        int[] baudrates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 }; //hardcoded baudrates from https://www.arduino.cc/en/Serial/Begin
        public MainWindow()
        {
            InitializeComponent();
            getAllPorts();

            foreach(string port in ports)
            {
                ComsComboBox.Items.Add(port);
                    if (ports[0] != null)
                {
                    ComsComboBox.SelectedItem = ports[ports.Length - 1];
                }
            }
            foreach(int baudrate in baudrates)
            {
                BaudrateComboBox.Items.Add(baudrate);
                if (baudrates[0] >0)
                {
                    BaudrateComboBox.SelectedItem = baudrates[baudrates.Length-1];
                }
            }

            void getAllPorts()
            {
                ports = SerialPort.GetPortNames();
            }
            variables.fileopened
        }
        private void connect()
        {
            
            string selectedPort = ComsComboBox.SelectedValue.ToString();
            int selectedBaudrate = int.Parse(BaudrateComboBox.SelectedItem.ToString());
            port = new SerialPort(selectedPort, selectedBaudrate, Parity.None, 8, StopBits.One);
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Load_file_partial second_view = new Load_file_partial();
            second_view.Show();
        }
    }
}
