using System;
using Microsoft.Win32;
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
using GrblEngineerProject.Machine;
using GrblEngineerProject.Partials;
using GrblEngineerProject;

namespace GrblEngineerProject
{
    public partial class MainWindow : Window
    {
        CNCConnection myCNC = App.myGlobalConnection;
        private int myTimer;
        public string fileName;
        public MainWindow()
        {
            InitializeComponent();
            myCNC.LineReceived += myCNCLineReceived;
            myCNC.LineSent += myCNCLineSent;
        }



        private void conDisconButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (!myCNC.isConnected)
            {
                myCNC.connect();
                serialLogBox.Items.Clear();
                serialLogBox.Items.Add("Serial communication on " + myCNC.PortName + " with speed of " + myCNC.ConnectionBaudRate + "bps");
                myTimer = 0;
            }
            else
            {
                myCNC.disconnect();
                serialLogBox.Items.Clear();
                myTimer = 0;
                conDisconButton.Content = "Connect";
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
            fileTextBox.Text = "File:" + fileName;
            if (myCNC.isConfigured)
            {
                conDisconButton.IsEnabled = true;

                if (myCNC.isConnected)
                {
                    machineStatusFlag.Fill = Brushes.Green;
                    myTimer++;
                    responseBlock.Foreground = Brushes.Green;
                    responseBlock.Text = "Connected!";
                    conDisconButton.Content = "Disconnect";
                    sendFileButton.IsEnabled = true;
                    if (!myCNC.isCNCIdle)
                    {
                        machineStatusFlag.Fill = Brushes.Red;
                        loadFileButton.IsEnabled = false;
                        sendFileButton.IsEnabled = false;
                    }
                    else
                    {
                        machineStatusFlag.Fill = Brushes.Green;
                        loadFileButton.IsEnabled = true;
                        sendFileButton.IsEnabled = true;
                    }
                }
                else
                {
                    responseBlock.Foreground = Brushes.Red;
                    responseBlock.Text = "No connection";
                    conDisconButton.Content = "Connect";
                    sendFileButton.IsEnabled = false;
                    machineStatusFlag.Fill = Brushes.DarkGray;
                }

            }
           
           
            time_ticks.Content = myTimer/2 + "s";

        }

        private void loadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opened_file = new OpenFileDialog();
            opened_file.DefaultExt = ".nc";
            opened_file.Filter = "Gcode file (*.nc)|*.nc";

            if (opened_file.ShowDialog() == true)
            {
                string filename = opened_file.FileName;
                try
                {
                    myCNC.LoadFile(System.IO.File.ReadAllLines(opened_file.FileName));
                    fileName = opened_file.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

           

        }

        public void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettings settingsView = new ConnectionSettings();
            settingsView.Show();
        }
        private void myCNCLineReceived(string obj)
        {
            serialLogBox.Items.Add(obj);
        }
        private void myCNCLineSent(string obj)
        {
            serialLogBox.Items.Add(obj);
        }

        private void sendFileButton_Click(object sender, RoutedEventArgs e)
        {
            myCNC.Work();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
