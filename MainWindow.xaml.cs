using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameRentalManagement.Models;
using GameRentalManagement.UserControls;
using Microsoft.VisualBasic.ApplicationServices;


namespace GameRentalManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameRentalDbContext con = new GameRentalDbContext();
        public int currentUserId;
        public string currentRole;
        public MainWindow(int userId, string role)
        {
            currentUserId = userId;
            currentRole = role;
            InitializeComponent();
            WelcomeMessage();
            AccessPermission();
            
        }
        private void WelcomeMessage()
        {
            txtLoginStatus.Text = $"Login Successful! Welcome,Game Rental Store's {currentRole}";
        }
        private void AccessPermission()
        {
            

            if (currentRole == "Admin")
            {
                txtAccessRights.Text = $"You have permission to manage information of: Rentals, Customers.";
            }
            else 
            {
                txtAccessRights.Text = $"You have permission to access to all features, include:Rentals, Customers, Reports, Accounts.";
                btn_Account.Visibility = Visibility.Collapsed;
            }
            

            
        }

        private void btn_Game_Click(object sender, RoutedEventArgs e)
        {
            notification.Visibility = Visibility.Collapsed;
            MainContent.Content = new GamePage();
        }
        private void btn_Rental_Click(object sender, RoutedEventArgs e)
        {
            notification.Visibility = Visibility.Collapsed;
            MainContent.Content = new RentalPage(currentUserId); ;
        }

        private void btn_Customer_Click(object sender, RoutedEventArgs e)
        {
            notification.Visibility = Visibility.Collapsed;
            MainContent.Content = new CustomerPage();
        }
        private void btn_Account_Click(object sender, RoutedEventArgs e)
        {
            notification.Visibility = Visibility.Collapsed;
            MainContent.Content = new AccountPage();
        }
        private void btn_Report_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReportPage(); 
        }

        private void btn_Logout_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}