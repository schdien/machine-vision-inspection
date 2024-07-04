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

namespace IndustrialCamera
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        LogginWindow parent;
        public LoginPage(LogginWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            
            if (ImageDatabase.ConnectAccount(UserNameBox.Text, PasswordBox.Password))
            {
                ImageDatabase.CreateTable();
                MainWindow mainWindow = new MainWindow();
                parent.Close();
                mainWindow.Show();
            }
            else
            {
                LoginErrorText.Visibility = Visibility.Visible;//用户名或密码错误
            }
        }
        private void ChangePasswordPage_Click(object sender, RoutedEventArgs e)
        {
            parent.ChangePasswordPage();
            LoginErrorText.Visibility=Visibility.Hidden;
            UserNameBox.Clear();
            PasswordBox.Clear();
        }
        private void CreateAccountPage_Click(object sender, RoutedEventArgs e)
        {
            parent.CreateAccountPage();
            LoginErrorText.Visibility = Visibility.Hidden;
            UserNameBox.Clear();
            PasswordBox.Clear();
        }
    }
}
