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
using GameRentalManagement.Models;

namespace GameRentalManagement
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    

    public partial class Login : Window
    {
        GameRentalDbContext con = new GameRentalDbContext();
        public Login()
        {
            GameRentalDbContext con = new GameRentalDbContext();
            InitializeComponent();
        }


        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and Password cannot be empty.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var user = con.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
                if (user != null)
                {
                   
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to log in: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }
    }
}
