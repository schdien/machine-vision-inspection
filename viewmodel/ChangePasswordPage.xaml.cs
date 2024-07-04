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
    /// ChangePasswordPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        LogginWindow parent;
        public ChangePasswordPage(LogginWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if(ImageDatabase.ChangePassword(UserNameBox.Text, OldPasswordBox.Password, NewPasswordBox.Password))
            {
                ChangeErrorText.Visibility = Visibility.Hidden;
                ChangeSuccessText.Visibility = Visibility.Visible;
            }
            else
            {
                ChangeSuccessText.Visibility = Visibility.Hidden;
                ChangeErrorText.Visibility = Visibility.Visible;
            }
        }

        private void LoginPage_Click(object sender, RoutedEventArgs e)
        {
            parent.LoginPage();
            ChangeSuccessText.Visibility = Visibility.Hidden;
            ChangeErrorText.Visibility = Visibility.Hidden;
            UserNameBox.Clear();
            OldPasswordBox.Clear();
            NewPasswordBox.Clear();
        }
        private void CreateAccountPage_Click(object sender, RoutedEventArgs e)
        {
            parent.CreateAccountPage();
            ChangeSuccessText.Visibility = Visibility.Hidden;
            ChangeErrorText.Visibility = Visibility.Hidden;
            UserNameBox.Clear();
            OldPasswordBox.Clear();
            NewPasswordBox.Clear();
        }
    }
}
