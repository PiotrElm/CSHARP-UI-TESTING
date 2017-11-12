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
using GrblEngineerProject.Partials;
using GrblEngineerProject;
using System.Globalization;

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
            myCNC.PositionReceived += myCNCPositionReceived;
            myCNC.LineSent += myCNCLineSent;
            PristineStatus();

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
                fileName = "";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(500);
            dt.Tick += dtTicker;
            dt.Start();
        }

        private void PristineStatus()
        {
            conDisconButton.IsEnabled = false;
            loadFileButton.IsEnabled = true;
            sendFileButton.IsEnabled = false;
            settingsButton.IsEnabled = true;
        }


        private void dtTicker(object sender, EventArgs e)
        {
            fileTextBox.Text = "File:" + fileName;
            if (myCNC.isConfigured)
            {
                conDisconButton.IsEnabled = true;

                if (myCNC.isConnected)
                {
                    myTimer++;
                    responseBlock.Foreground = Brushes.Green;
                    responseBlock.Text = "Connected!";
                    conDisconButton.Content = "Disconnect";
                    if(fileName != "") { 
                        if (GlobalVariables.MachineStatus == "Idle")
                        {
                            sendFileButton.IsEnabled = true;
                        }
                        else
                        {
                            sendFileButton.IsEnabled = false;
                        }
                    }
                    if (myCNC.type == "file")
                    {
                        loadFileButton.IsEnabled = false;
                    }
                    else
                    {
                        loadFileButton.IsEnabled = true;
                    }
                }
                else
                {
                    fileName = "";
                    responseBlock.Foreground = Brushes.Red;
                    responseBlock.Text = "No connection";
                    conDisconButton.Content = "Connect";
                }

            }


            time_ticks.Content = myTimer / 2 + "s";

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
        private static void scrollDown(ListBox controlName)
        {
            controlName.SelectedIndex = controlName.Items.Count - 1;
            controlName.ScrollIntoView(controlName.SelectedItem);
        }


        private void myCNCLineReceived(string obj)
        {
            serialLogBox.Items.Add(obj);
            scrollDown(serialLogBox);
        }

        private void myCNCLineSent(string obj)
        {
            serialLogBox.Items.Add(obj);
            scrollDown(serialLogBox);
        }

        private void myCNCPositionReceived(string obj)
        {
            PositionTextBlock.Text = obj;
            SetGlobalCoordinates(obj);
            UpdateCoordinatesView();
        }

        private void UpdateCoordinatesView()
        {
            if(GlobalVariables.MachinePositionAsPoint.x != null && GlobalVariables.MachinePositionAsPoint.y != null && GlobalVariables.MachinePositionAsPoint.z != null
                && GlobalVariables.WorkingPositionAsPoint.x != null && GlobalVariables.WorkingPositionAsPoint.x != null && GlobalVariables.WorkingPositionAsPoint.x != null) {
            xMachinePostion.Text = GlobalVariables.MachinePositionAsPoint.x;
            yMachinePostion.Text = GlobalVariables.MachinePositionAsPoint.y;
            zMachinePostion.Text = GlobalVariables.MachinePositionAsPoint.z;
            xWorkingPostion.Text = GlobalVariables.WorkingPositionAsPoint.x;
            yWorkingPostion.Text = GlobalVariables.WorkingPositionAsPoint.y;
            zWorkingPostion.Text = GlobalVariables.WorkingPositionAsPoint.z;
            Thickness m = PositionPointer.Margin;
            var styles = NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowDecimalPoint;
            m.Left = -2.5 + Double.Parse(GlobalVariables.WorkingPositionAsPoint.x, styles, System.Globalization.NumberFormatInfo.InvariantInfo) /2.5;
            m.Top = 198.5-Double.Parse(GlobalVariables.WorkingPositionAsPoint.y, styles, System.Globalization.NumberFormatInfo.InvariantInfo)/2.5;
            PositionPointer.Margin = m;
            }
        }

        private void SetGlobalCoordinates(string obj){
            if (obj.StartsWith("<A"))
            {
                GlobalVariables.MachineStatus = "ALARM";
                return;
            }
            GlobalVariables.PositionAnswer = obj.Clone().ToString();
            int statusStart = GlobalVariables.PositionAnswer.IndexOf('<') + 1;
            int statusEnd = GlobalVariables.PositionAnswer.IndexOf(',') - 1;
            int statusLenght = statusEnd - statusStart + 1;
            int machinePosStart = GlobalVariables.PositionAnswer.IndexOf(':') + 1;
            int machinePosEnd = GlobalVariables.PositionAnswer.IndexOf("WPos") - 1;
            int machinePosLenght = machinePosEnd - machinePosStart;
            int workingPosStart = GlobalVariables.PositionAnswer.LastIndexOf(":") + 1;
            int workingPosEnd = GlobalVariables.PositionAnswer.IndexOf(">");
            int workingPosLenght = workingPosEnd - workingPosStart;
            char[] machineStatusAsCharArray = new char[statusLenght];
            char[] machinePositionAsCharArray = new char[machinePosLenght];
            char[] workingPositionAsCharArray = new char[workingPosLenght];
            GlobalVariables.PositionAnswer.CopyTo(statusStart, machineStatusAsCharArray, 0, statusLenght);
            GlobalVariables.PositionAnswer.CopyTo(machinePosStart, machinePositionAsCharArray, 0, machinePosLenght);
            GlobalVariables.PositionAnswer.CopyTo(machinePosEnd + 6, workingPositionAsCharArray, 0, workingPosLenght);
            string tempstatus = new string(machineStatusAsCharArray);
            string machinePosition = new string(machinePositionAsCharArray);
            string workingPosition = new string(workingPositionAsCharArray);
            GlobalVariables.MachineStatus = tempstatus;
            GlobalVariables.MachinePos = machinePosition;
            GlobalVariables.WorkPos = workingPosition;
            try {
                GlobalVariables.MachinePositionAsPoint.x = GlobalVariables.MachinePos.Substring(0, GlobalVariables.MachinePos.IndexOf(","));
                GlobalVariables.MachinePositionAsPoint.y = GlobalVariables.MachinePos.Substring(GlobalVariables.MachinePos.IndexOf(",")+1, GlobalVariables.MachinePos.LastIndexOf(",")- GlobalVariables.MachinePos.IndexOf(",")-1);
                GlobalVariables.MachinePositionAsPoint.z = GlobalVariables.MachinePos.Substring(GlobalVariables.MachinePos.LastIndexOf(",")+1, GlobalVariables.MachinePos.Length- GlobalVariables.MachinePos.LastIndexOf(",")-1);
                GlobalVariables.WorkingPositionAsPoint.x = GlobalVariables.WorkPos.Substring(0, GlobalVariables.WorkPos.IndexOf(","));
                GlobalVariables.WorkingPositionAsPoint.y = GlobalVariables.WorkPos.Substring(GlobalVariables.WorkPos.IndexOf(",") + 1, GlobalVariables.WorkPos.LastIndexOf(",") - GlobalVariables.WorkPos.IndexOf(",") - 1);
                GlobalVariables.WorkingPositionAsPoint.z = GlobalVariables.WorkPos.Substring(GlobalVariables.WorkPos.LastIndexOf(",") + 1, GlobalVariables.WorkPos.Length - GlobalVariables.WorkPos.LastIndexOf(",") - 1);
            }
            catch
            {
            }
            
        }
        private void sendFileButton_Click(object sender, RoutedEventArgs e)
        {
            myCNC.type = "file";
        }


        private void ResetZeroButton_Click_1(object sender, RoutedEventArgs e)
        {
            myCNC.resetZero();
        }
        private void ManualCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key  == Key.Enter)
            {
                myCNC.manualCommand(ManualCommand.Text);
            }
        }
    }
}
