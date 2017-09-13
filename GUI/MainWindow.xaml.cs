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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string From = "";
        private string To = "";
        private string Status = "Ready";

        public string FromPath
        {
            get { return From; }
            set
            {
                From = value;

                OnPropertyChanged("FromPath");
            }
        }

        public string ToPath
        {
            get { return To; }
            set
            {
                To = value;

                OnPropertyChanged("ToPath");
            }
        }

        public string StatusText
        {
            get { return Status; }
            set
            {
                Status = value;

                OnPropertyChanged("StatusText");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void Open_File(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "", // Default file name
                DefaultExt = ".dbc", // Default file extension
                Filter = "CANdb Network (*.dbc)|*.dbc" // Filter files by extension
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
            FromPath = filename;


        }

        async void Convert(object sender, RoutedEventArgs e)
        {
            using (StreamReader reader = new StreamReader(FromPath))
            {
                DbcParser parser = new DbcParser(reader);

                DBC dbc = await Task.Run(() => parser.Parse());

                StatusText = dbc.messages[0].name;
            }
        }
    }
}
