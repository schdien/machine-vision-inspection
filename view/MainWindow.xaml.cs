using System;
using System.Collections.Generic;
using System.IO;
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
using GxIAPINET;

namespace IndustrialCamera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            //ImageDatabase.Create();
            //ImageDatabase.Connect();
            //ImageDatabase.CreateTable();
            devicePage = new DevicePage();
            dataPage = new DataPage();
            InitializeComponent();
            dataInfoBadge.Visibility = Visibility.Collapsed;
            Frame1.Navigate(devicePage);
            Closed += WindowClose_Click;
            Device.ImageListViewChanged += DataInfoBadgeDisp;

        }
        DataPage dataPage;
        DevicePage devicePage;

        void WindowClose_Click(object sender, EventArgs e)
        {
            ImageDatabase.Disconnect();
        }
        private void DataInfoBadgeDisp(object sender, EventArgs e)
        {
            dataInfoBadge.Visibility = Visibility.Visible;
        }
        private void MenuItem_Click(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewItemInvokedEventArgs args)
        {

            switch (args.InvokedItemContainer.Name)
            {
                case "deviceNavi":
                    Frame1.Navigate(devicePage);
                    break;
                case "dataNavi":
                    Frame1.Navigate(dataPage);
                    dataInfoBadge.Visibility = Visibility.Collapsed;
                    break;
                case "aboutNavi":
                    Frame1.Navigate(typeof(AboutPage));
                    break;
            }
        }
    } 
}
