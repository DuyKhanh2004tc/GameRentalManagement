using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using GameRentalManagement.Models;
using GameRentalManagement.Utils;


namespace GameRentalManagement.UserControls
{
    /// <summary>
    /// Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : UserControl
    {
        GameRentalDbContext con = new();

        public AccountPage()
        {
            InitializeComponent();
            LoadAllUsers();
            ClearForm();
        }

        private void LoadAllUsers()
        {
            dgUserList.ItemsSource = con.Users.ToList();
        }
        private void ClearForm()
        {
            dgUserList.SelectedItem = null;
            txtUserId.Text = "";
            txtUsername.Text = "";           
            txtPassword.Password = "";
            cbRole.SelectedIndex = -1;
        }
        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtFilter.Text.ToLower();
            var filtered = con.Users
                .Where(u => u.Username.ToLower().Contains(keyword) || u.Role.ToLower().Contains(keyword))
                .ToList();

            dgUserList.ItemsSource = filtered;
        }

        private void dgUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUserList.SelectedItem is User selected)
            {
                txtUserId.Text = selected.UserId.ToString();
                txtUsername.Text = selected.Username;              
                txtPassword.Password = "";
                cbRole.Text = selected.Role;
            }
        }
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                cbRole.SelectedItem == null)
            {
                MessageBox.Show("Please fill all required fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            string pattern = @"^(?=.*[A-Z])(?=.*\d).{6,}$";
            if (!Regex.IsMatch(password, pattern))
            {
                MessageBox.Show("Password must be at least 6 characters and include at least 1 uppercase letter and 1 number.",
                                "Weak Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (con.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Username already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User newUser = new()
            {
                Username = username,
                Password = PasswordEncryption.HashPassword(password) , 
                Role = ((ComboBoxItem)cbRole.SelectedItem).Content.ToString()
            };

            con.Users.Add(newUser);
            con.SaveChanges();
            MessageBox.Show("User added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAllUsers();
            ClearForm();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgUserList.SelectedItem is not User selected) return;

            var user = con.Users.FirstOrDefault(u => u.UserId == selected.UserId);
            if (user == null) return;

            user.Username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            string pattern = @"^(?=.*[A-Z])(?=.*\d).{6,}$";
            if (!Regex.IsMatch(password, pattern))
            {
                MessageBox.Show("Password must be at least 6 characters and include at least 1 uppercase letter and 1 number.",
                                "Weak Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.Password = PasswordEncryption.HashPassword(password);
            }

            user.Role = ((ComboBoxItem)cbRole.SelectedItem).Content.ToString();

            con.SaveChanges();
            MessageBox.Show("User updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAllUsers();
            ClearForm();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgUserList.SelectedItem is not User selected)
            {
                MessageBox.Show("Please select a user to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure to delete this user?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                con.Users.Remove(selected);
                con.SaveChanges();
                MessageBox.Show("User deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllUsers();
                ClearForm();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
    }
}
