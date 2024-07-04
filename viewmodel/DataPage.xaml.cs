using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using ModernWpf.Controls;

namespace IndustrialCamera
{
    /// <summary>
    /// DataPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataPage : System.Windows.Controls.Page
    {
        public DataPage()
        {
            InitializeComponent();
            imageListView.ItemsSource = ImageDatabase.Table;
            Device.ImageListViewChanged += ImageListViewChangedHandler;
        }


        //双击列表项时使用系统图片浏览器打开图片
        void ImageListView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var imageInfo = ((FrameworkElement)e.OriginalSource).DataContext as DataRowView;
  
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            //设置图片的路径
            process.StartInfo.FileName = imageInfo["path"].ToString();
            //设置进程运行参数，这里以最大化窗口方法显示图片。    
            process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll,ImageView_Fullscreen";
            process.Start();
            process.Close();
        }
        private void ImageListViewChangedHandler(object sender ,EventArgs e)
        {
            imageListView.ItemsSource = ImageDatabase.Table;
        }

        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            var imageInfo = button.DataContext as DataRowView;
            if(imageInfo != null)
            { 
                string path = imageInfo["path"].ToString();
                ImageDatabase.DeleteFromTable(path);
                File.Delete(path);
                imageListView.ItemsSource = ImageDatabase.Table;
            }
        }
    }
}
