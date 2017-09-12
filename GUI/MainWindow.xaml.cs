using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DbcLib.DBC.Model;
using DbcLib.DBC.Parser;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        async void Open_File(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Document", // Default file name
                DefaultExt = ".dbc", // Default file extension
                Filter = "DBC file (*.dbc)|*.dbc" // Filter files by extension
            };

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true)
            {
                return;
            }

            // Save document
            string filename = dlg.FileName;

            using (StreamReader reader = new StreamReader(filename))
            {
                DbcParser parser = new DbcParser(reader);
                //var job = await Task.Run(() => parser.Parse());
                var job =  parser.Parse();
                DbcMessage.Text = String.Join(",", job.messages[0].signals[0].receivers);
            }
        }
    }
}
