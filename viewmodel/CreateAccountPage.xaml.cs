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
    /// CreateAccountPage.xaml 的交互逻辑
    /// </summary>
    
    public partial class CreateAccountPage : Page
    {
        LogginWindow parent;
        public CreateAccountPage(LogginWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if(ImageDatabase.CreateAccount(UserNameBox.Text, PasswordBox.Password))
            {
                RegisterErrorText.Visibility = Visibility.Hidden;
                RegisterSuccessText.Visibility = Visibility.Visible;
            }
            else
            {
                RegisterErrorText.Visibility = Visibility.Visible;
                RegisterSuccessText.Visibility = Visibility.Hidden;
            }
        }
        private void LoginPage_Click(object sender, RoutedEventArgs e)
        {
            parent.LoginPage();
            RegisterErrorText.Visibility = Visibility.Hidden;
            RegisterSuccessText.Visibility = Visibility.Hidden;
            UserNameBox.Clear();
            PasswordBox.Clear();
        }
        private void ChangePasswordPage_Click(object sender, RoutedEventArgs e)
        {
            parent.ChangePasswordPage();
            RegisterErrorText.Visibility = Visibility.Hidden;
            RegisterSuccessText.Visibility = Visibility.Hidden;
            UserNameBox.Clear();
            PasswordBox.Clear();
        }
    }
}
