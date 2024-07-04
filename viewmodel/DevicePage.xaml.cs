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
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
    /// <summary>
    /// DevicePage.xaml 的交互逻辑
    /// </summary>
    public partial class DevicePage : Page
    {
        public DevicePage()
        {
            InitializeComponent();
            devicesInfo = new DevicesInfo();
            DeviceListUpdate_Click(null,null);
            deviceListView.ItemsSource = devicesInfo.DeviceList;
            imageListView.ItemsSource = devicesInfo.DeviceList;
        }
        DevicesInfo devicesInfo;
        private void DeviceListUpdate_Click(object sender ,RoutedEventArgs e)
        {
            devicesInfo.UpdateDeviceInfoList();
        }

    }
}
