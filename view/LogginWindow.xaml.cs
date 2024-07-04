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
using System.Windows.Shapes;

namespace IndustrialCamera
{
    /// <summary>
    /// LogginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogginWindow : Window
    {
        LoginPage loginPage;
        ChangePasswordPage changePasswordPage;
        CreateAccountPage createAccountPage;

        public LogginWindow()
        {
            InitializeComponent();
            loginPage = new LoginPage(this);
            changePasswordPage = new ChangePasswordPage(this);
            createAccountPage = new CreateAccountPage(this);
            Frame0.Navigate(loginPage);
        }

        public void ChangePasswordPage()
        {
            Frame0.Navigate(changePasswordPage);
        }
        public void LoginPage()
        {
            Frame0.Navigate(loginPage);
        }
        public void CreateAccountPage()
        {
            Frame0.Navigate(createAccountPage);
        }
    }
}
