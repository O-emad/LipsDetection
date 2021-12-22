using AzureCognitiveServices;
using AzureCognitiveServices.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static AzureCognitiveServices.Client.CognitiveService;

namespace LipsDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        public CognitiveService Service { get; set; }
        public CognitiveService ServiceInstance2 { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Service = CognitiveServiceBuilder.Create()
                            .EnableResultOverlay()
                            .Build(Dispatcher);

            Service.LatestResultsToDisplay.PropertyChanged += OnFaceRectangleChange;
            //create containers in wpf for the left image "unprocessed images", right image "processed images", and a message area to view response messages
            //register them to the services following properties. 
            Service.MessageArea = MessageArea;
            //Service.LeftImage = null;
            Service.RightImage = RightImage;

            ServiceInstance2 = CognitiveServiceBuilder.Create()
                                   .Build(Dispatcher);
            ServiceInstance2.MessageArea = MessageArea;
            ServiceInstance2.RightImage = LeftImage;
            ServiceInstance2.SetAppMode(AppMode.LocalDetection);

        }

        private void OnFaceRectangleChange(object? sender, PropertyChangedEventArgs e)
        {
            MessageArea.Text = Service.LatestResultsToDisplay.Faces.FirstOrDefault()?.CalculateMouthRectangle()?.X.ToString();
           
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            slider_AnalysisInterval.Value = 100;
        }


        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CameraList.HasItems)
            {
                MessageArea.Text = "No cameras found; cannot start processing";
                return;
            }
            MessageArea.Text = "";
            // How often to analyze. 
            await Service.StartProcessing(TimeSpan.FromMilliseconds(slider_AnalysisInterval.Value), 1); //CameraList.SelectedIndex);
            await ServiceInstance2.StartProcessing(TimeSpan.FromMilliseconds(slider_AnalysisInterval.Value), 0);
        }



        private async void stopButton_Click(object sender, RoutedEventArgs e)
        {
            await Service.StopProcessing();
            await ServiceInstance2.StopProcessing();
        }

        private void ModeList_Loaded(object sender, RoutedEventArgs e)
        {
            var modes = (AppMode[])Enum.GetValues(typeof(AppMode));
            var comboBox = sender as ComboBox ?? throw new ArgumentNullException(nameof(sender));
            comboBox.ItemsSource = modes.Select(m => m.ToString());
            comboBox.SelectedIndex = ((int)AppMode.LocalDetection);
        }

        private void ModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox ??throw new ArgumentNullException(nameof(sender));
            var modes = (AppMode[])Enum.GetValues(typeof(AppMode));
            Service.SetAppMode(modes[comboBox.SelectedIndex]);
            checkBox.IsChecked = Service.FuseClientRemoteResults;
        }

        private void CameraList_Loaded(object sender, RoutedEventArgs e)
        {
            int numCameras = Service.GetNumberOfAvailableCameras();
            if (numCameras == 0)
            {
                MessageArea.Text = "No cameras found!";
            }
            var comboBox = sender as ComboBox ?? throw new ArgumentNullException(nameof(sender));
            comboBox.ItemsSource = Enumerable.Range(0, numCameras).Select(i => string.Format("Camera {0}", i + 1));
            comboBox.SelectedIndex = 0;
        }




        private void checkBox_SelectionChange(object sender, RoutedEventArgs e)
        {
            if(checkBox.IsChecked is not null)
                Service.FuseRemoteResultsSetting(checkBox.IsChecked.Value);
        }

        private void slider_AnalysisInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Service?.TriggerAnalysisOnInterval(TimeSpan.FromMilliseconds(slider_AnalysisInterval.Value));
        }

        private bool disposedValue = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Service?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
