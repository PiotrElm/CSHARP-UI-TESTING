using Microsoft.Win32;
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

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy Connection_details.xaml
    /// </summary>
    public partial class Load_file_partial : Window
    {
        public Load_file_partial()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opened_file = new OpenFileDialog();
            opened_file.DefaultExt = ".nc";
            opened_file.Filter = "Gcode file (*.nc)|*.nc";
            if (opened_file.ShowDialog() == true)
            {
                string filename = opened_file.FileName;
            }
        }
    }
    class global_data
    {
    }
}
