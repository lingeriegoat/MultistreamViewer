using Microsoft.Web.WebView2.Wpf;
using StreamMultiview.Properties;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;


namespace StreamMultiview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int WM_EXITSIZEMOVE = 0x0232;


        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                int useDark = 1;
                DwmSetWindowAttribute(
                    hwnd,
                    DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref useDark,
                    sizeof(int));
            };

            LoadSavedUrls();
            LoadWindowSizeAndState();
            LoadGridSize();

            // Initialize WebView2 controls
            wb1.EnsureCoreWebView2Async();
            wb2.EnsureCoreWebView2Async();
            wb3.EnsureCoreWebView2Async();
            wb4.EnsureCoreWebView2Async();

            NavigateOnLoad();
        }

        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_EXITSIZEMOVE)
            {
                SaveWindowSizeAndState();
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveGridSize();
            SaveWindowSizeAndState();
            SaveSettings();
        }

        private void SaveWindowSizeAndState()
        {
            Rect bounds = this.RestoreBounds;

            Settings.Default.WindowLeft = bounds.Left;
            Settings.Default.WindowTop = bounds.Top;
            Settings.Default.WindowWidth = bounds.Width;
            Settings.Default.WindowHeight = bounds.Height;
            Settings.Default.WindowState = (int)this.WindowState;
        }

        private void LoadWindowSizeAndState()
        {
            this.Left = Settings.Default.WindowLeft;
            this.Top = Settings.Default.WindowTop;
            this.Width = Settings.Default.WindowWidth;
            this.Height = Settings.Default.WindowHeight;

            if (Width < 100)
            {
                this.Width = 1920;
                this.Height = 1080;
            }

            if (Left < -2000 || Left > SystemParameters.VirtualScreenWidth)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            this.WindowState = (WindowState)Settings.Default.WindowState;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveGridSize();
            SaveSettings();
        }

        private void SaveGridSize()
        {
            Settings.Default.LeftWidth = MainGrid.ColumnDefinitions[0].Width.Value;
            Settings.Default.TopHeight = MainGrid.RowDefinitions[0].Height.Value;
        }

        private void LoadGridSize()
        {
            MainGrid.ColumnDefinitions[0].Width = new GridLength(Settings.Default.LeftWidth, GridUnitType.Pixel);
            MainGrid.RowDefinitions[0].Height = new GridLength(Settings.Default.TopHeight, GridUnitType.Pixel);
        }

        private void LoadSavedUrls()
        {
            AddressBar1.Text = Settings.Default.url1;
            AddressBar2.Text = Settings.Default.url2;
            AddressBar3.Text = Settings.Default.url3;
            AddressBar4.Text = Settings.Default.url4;
        }

        private void SaveUrls()
        {
            Settings.Default.url1 = AddressBar1.Text;
            Settings.Default.url2 = AddressBar2.Text;
            Settings.Default.url3 = AddressBar3.Text;
            Settings.Default.url4 = AddressBar4.Text;
            SaveSettings();
        }
        private void SaveSettings()
        {
            Settings.Default.Save();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            int browser = int.Parse(((Button)sender).Tag.ToString());

            Navigate(browser);
        }

        private void NavigateOnLoad()
        {
            if (!string.IsNullOrEmpty(AddressBar1.Text))
            {
                Navigate(1);
            }
            if (!string.IsNullOrEmpty(AddressBar2.Text))
            {
                Navigate(2);
            }
            if (!string.IsNullOrEmpty(AddressBar3.Text))
            {
                Navigate(3);
            }
            if (!string.IsNullOrEmpty(AddressBar4.Text))
            {
                Navigate(4);
            }
        }


        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (sender == AddressBar1)
            {
                Navigate(1);
            }
            else if (sender == AddressBar2)
            {
                Navigate(2);
            }
            else if (sender == AddressBar3)
            {
                Navigate(3);
            }
            else if (sender == AddressBar4)
            {
                Navigate(4);
            }
        }

        private void Navigate(int browserNumber)
        {
            TextBox addressBar;
            WebView2 browser;

            switch (browserNumber)
            {
                case 1:
                    addressBar = AddressBar1;
                    browser = wb1;
                    break;

                case 2:
                    addressBar = AddressBar2;
                    browser = wb2;
                    break;

                case 3:
                    addressBar = AddressBar3;
                    browser = wb3;
                    break;

                default:
                    addressBar = AddressBar4;
                    browser = wb4;
                    break;
            }

            string url = addressBar.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            SaveUrls();

            browser.Source = new Uri(url);
        }


    }
}