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
using Npgsql;

namespace CDTP_serial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        NpgsqlConnection connection;
        SerialPort sp;
        TimeSimulator timeSimulator;

        public MainWindow()
        {
            InitializeComponent();
            RefleshPortNames();

            timeSimulator = new TimeSimulator(currentdate , calender);
            sp = new SerialPort();

            connection = new NpgsqlConnection();


 

            timeSimulator.Tick += () =>
            {
                if(sp.IsOpen)
                {
                    log("Simulator time:"+ timeSimulator.Today.ToLongDateString(), LogModality.Info);
                    sp.WriteLine("getValues");

                }
            };

        }

        public void RefleshPortNames()
        {
            log("Refleshing port names...", LogModality.Info);

            comPortComboBox.ItemsSource = SerialPort.GetPortNames();
            if (comPortComboBox.Items.Count != 0)
                comPortComboBox.SelectedIndex = 0;
        }


        public enum LogModality
        {
            Inbound,
            Outbound,
            Error,
            Info
        }
        public void log (string text , LogModality modality)
        {
            var baseText = string.Format("[{0}][{1}]:", DateTime.Now.ToLongTimeString() , modality.ToString());
            var logText  = string.Format("{0}\r", text);
            richTextBox.Dispatcher.Invoke(() =>
            {
                // richTextBox.AppendText(str);


                TextRange rangeOfText1 = new TextRange(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                rangeOfText1.Text = baseText;

                SolidColorBrush sb = null ;
                switch (modality)
                {
                    case LogModality.Inbound: sb = Brushes.DarkGreen;
                        break;
                    case LogModality.Outbound: sb = Brushes.Green; 
                        break;
                    case LogModality.Error: sb = Brushes.Red; 
                        break;
                    case LogModality.Info: sb = Brushes.Blue; 
                        break;
                    default:
                        break;
                }

                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, sb);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange rangeOfText2 = new TextRange(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                rangeOfText2.Text = logText;
                rangeOfText2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                rangeOfText2.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);



                richTextBox.ScrollToEnd();

            });

        }


        public bool Connect()
        {
            if(comPortComboBox.SelectedItem != null)
            {
               
                var comName = comPortComboBox.SelectedItem.ToString();
                var baudStr = (baudRateComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                var baudRate = int.Parse(baudStr);

                log("Trying to connect on port " + comName + " : " + baudRate  , LogModality.Info);

                sp.PortName = comName;
                sp.BaudRate = baudRate;
                sp.ReceivedBytesThreshold = 1;
                sp.ErrorReceived += Sp_ErrorReceived;
                sp.DataReceived += Sp_DataReceived;
                sp.PinChanged += Sp_PinChanged;
                sp.Open();

                if (sp.IsOpen)
                {
                    connectButton.Content = "Disconnect";
                    statusText.Foreground = Brushes.Green;
                    statusText.Content = "Connected";
                    log("Connected " + comName + " : " + baudRate, LogModality.Info);
                    return true;
                }
                else
                {
                    connectButton.Content = "Connect";
                    statusText.Foreground = Brushes.Gray;
                    statusText.Content = "Not Connected";
                    log("Cant connect " + comName + " : " + baudRate, LogModality.Error);

                    return false;
                }
            }
            return false;
        }
        public void Disconnect()
        {
            sp.Close();

            log("Disconnected " + sp.PortName + " : " + sp.BaudRate, LogModality.Info);

            connectButton.Content = "Connect";
            statusText.Foreground = Brushes.Gray;
            statusText.Content = "Not Connected";
        }

        private void Sp_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            log("Com port Pin Changed: " + e.ToString(), LogModality.Info);
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var read = sp.ReadTo("\n");

            var lines = read.Split(';');

            foreach( var line in lines)
            {
                if(string.IsNullOrEmpty(line)) continue;

                log("Data Received: " + line.TrimEnd('\n'), LogModality.Inbound);



                // get data as   id;energyUsage;freeEnergyUsage;
                var values = line.Split(',');
                if (values.Length < 3) return;

                var id = int.Parse(values[0]);
                var energyUsage = double.Parse(values[1]);
                var freeEnergyUsage = double.Parse(values[2]);

                // calculate cost
                double cost = (energyUsage-freeEnergyUsage) * 1.0f;

                if(connection.State == System.Data.ConnectionState.Open)
                { 
                using (var cmd = new NpgsqlCommand("INSERT INTO public.usage( timestamp, energyusage, freeusage, cost, deviceid)VALUES( @timestamp, @energyUsage, @freeEnergyUsage, @cost, @deviceid)", connection))
                {
                    cmd.Parameters.AddWithValue("timestamp", timeSimulator.Today);
                    cmd.Parameters.AddWithValue("energyUsage", energyUsage);
                    cmd.Parameters.AddWithValue("freeEnergyUsage", freeEnergyUsage);
                    cmd.Parameters.AddWithValue("cost", cost);
                    cmd.Parameters.AddWithValue("deviceid", id);
                    int res =  cmd.ExecuteNonQuery();
                    if(res == 0)
                    {
                        log("SQL insert Error , deviceid:" + id , LogModality.Error );
                    }
                }
                }else
                {
                    log("Sql connetion not available", LogModality.Error);
                }
            }







        }

        private void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            log("Error received: " + e.ToString(), LogModality.Error);

        }






        #region ui actions
        private void refleshPortNames_Click(object sender, RoutedEventArgs e)
        {
            RefleshPortNames();
        }

     

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if(sp != null && sp.IsOpen)
            {
                Disconnect();

                baudRateComboBox.IsEnabled = true;
                comPortComboBox.IsEnabled = true;

            }else
            {
                if(Connect())
                {
                    baudRateComboBox.IsEnabled = false;
                    comPortComboBox.IsEnabled = false;
                }
            }
 
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (sp != null && sp.IsOpen)
            {
                log(inputTextBox.Text, LogModality.Outbound);

                sp.WriteLine(inputTextBox.Text);


            }
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                sendButton_Click(this, null);
                inputTextBox.Text = "";
            }
        }


        private void toggleSimulation_Click(object sender, RoutedEventArgs e)
        {
            if(timeSimulator.isEnabled)
            {
               

                timeSimulator.Stop();
                (sender as Button).Content = "Start";
            }
            else
            {
                timeSimulator.Start();
                (sender as Button).Content = "Stop";
            }
        }

        #endregion

        private void dateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(timeSimulator != null)
            {
                timeSimulator.IntervalSecs = (int)e.NewValue;
                simulationSpeedLabel.Content = string.Format("Simulation Speed(1 day = {0} seconds)", timeSimulator.IntervalSecs);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                log("Sql connection closed.", LogModality.Info);


                sqlusernameTextBox.IsEnabled = true;
                sqlpasswordTextBox.IsEnabled = true;
                sqlConnectLabel.Content = "Connect";
                sqlConnectLabel.Foreground = Brushes.Black;
            }
            else
            {
                connection.ConnectionString = string.Format("Server=127.0.0.1;Database=CDTP;User Id={0};Password={1}; " , sqlusernameTextBox.Text, sqlpasswordTextBox.Text);
                try
                {
                    connection.Open();

                }
                catch (Exception ex)
                {

                    //MessageBox.Show( ex.Message,"Connection Error");
                    log("Can not connect Sql, " + ex.Message, LogModality.Error);
                    sqlConnectLabel.Content = "Can not connect, try again";
                    sqlConnectLabel.Foreground = Brushes.Red;
 
                    return;
                }
                log("Sql connection opened.", LogModality.Info);
                sqlConnectLabel.Content = "Connected";
                sqlConnectLabel.Foreground = Brushes.Green;
                sqlusernameTextBox.IsEnabled = false;
                sqlpasswordTextBox.IsEnabled = false;
            }
        }
    }
}
