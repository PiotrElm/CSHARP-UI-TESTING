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
using System.Windows.Shapes;
using GrblEngineerProject.Machine;
using System.IO.Ports;

namespace GrblEngineerProject.Partials
{
    /// <summary>
    /// Logika interakcji dla klasy ConnectionSettings.xaml
    /// </summary>
    public partial class ConnectionSettings : Window
    {
        CNCConnection myCNC = App.myGlobalConnection;
        String[] ports;
        int[] baudrates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 }; //hardcoded baudrates from https://www.arduino.cc/en/Serial/Begin
        

        public ConnectionSettings()
        {
            InitializeComponent();
            getAllPorts();
            if (myCNC.isConnected)
            {
                saveSettingsButton.IsEnabled = false;
            }
            else
            {
                saveSettingsButton.IsEnabled = true;
            }
        }

        void getAllPorts()
        {
            ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                ComsComboBox.Items.Add(port);
                if (ports[0] != null && myCNC.isConfigured == false)
                {
                    ComsComboBox.SelectedItem = ports[ports.Length - 1];
                }
                else
                {
                    ComsComboBox.SelectedItem = myCNC.PortName;
                }
            }
            foreach (int baudrate in baudrates)
            {
                BaudrateComboBox.Items.Add(baudrate);
                if (baudrates[0] > 0 && myCNC.isConfigured == false)
                {
                    BaudrateComboBox.SelectedItem = baudrates[baudrates.Length - 1];
                }
                else
                {
                    BaudrateComboBox.SelectedItem = myCNC.ConnectionBaudRate;
                }
            }
        }


        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            ComsComboBox.Items.Clear();
            BaudrateComboBox.Items.Clear();
            getAllPorts();
            if(myCNC.isConfigured == true)
            {
                ComsComboBox.SelectedItem = myCNC.PortName;
                BaudrateComboBox.SelectedItem = myCNC.ConnectionBaudRate;
            }
        }

        public void saveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            myCNC.ConnectionBaudRate = int.Parse(BaudrateComboBox.SelectedItem.ToString());
            myCNC.PortName = ComsComboBox.SelectedValue.ToString();
            myCNC.saveSettings();
            this.Hide();
        }
    }
}
