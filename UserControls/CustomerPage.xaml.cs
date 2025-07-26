using System;
using System.Collections.Generic;
using System.Drawing;
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
using Microsoft.EntityFrameworkCore;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace GameRentalManagement.UserControls
{
    /// <summary>
    /// Interaction logic for CustomerPage.xaml
    /// </summary>

    public partial class CustomerPage : UserControl
    {
        GameRentalDbContext con = new GameRentalDbContext();
        public CustomerPage()
        {
            InitializeComponent();
            LoadAllCustomers();
            ClearForm();
        }

        private void LoadAllCustomers()
        {
            dgCustomerList.ItemsSource = con.Customers.ToList();
        }

        private void FilterCustomers(string keywords)
        {
            var customers = con.Customers
                .Where(c => c.FullName.ToLower().Contains(keywords)
                         || c.Phone.Contains(keywords)
                         || c.Email.ToLower().Contains(keywords)
                         || c.Address.ToLower().Contains(keywords))
                .ToList();
            dgCustomerList.ItemsSource = customers;
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keywords = txtFilter.Text.ToLower();
            FilterCustomers(keywords);
        }

        private void dgCustomerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomerList.SelectedItem is Customer selected)
            {
                txtCustomerId.Text = selected.CustomerId.ToString();
                txtFullName.Text = selected.FullName;
                txtPhone.Text = selected.Phone;
                txtEmail.Text = selected.Email;
                txtAddress.Text = selected.Address;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill all the fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string namePattern = @"^([\p{L}]+[\s]+){1,}[\p{L}]+$";
            if (!Regex.IsMatch(txtFullName.Text.Trim(), namePattern))
            {
                MessageBox.Show("Full name must contain at least two words and only letters (Vietnamese supported).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string phonePattern = @"^0\d{9}$";
            if (!Regex.IsMatch(txtPhone.Text.Trim(), phonePattern))
            {
                MessageBox.Show("Phone number is invalid. It should be 10 digits and start with 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(txtEmail.Text.Trim(), emailPattern))
            {
                MessageBox.Show("Email format is invalid.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool isExisted = con.Customers.Any(c =>
                                c.FullName.Equals(txtFullName.Text.Trim()) &&
                                c.Phone.Equals(txtPhone.Text.Trim()));

            if (isExisted)
            {
                MessageBox.Show("Customer already exists.", "Duplicate Customer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Customer newCustomer = new Customer
            {
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };

            con.Customers.Add(newCustomer);
            con.SaveChanges();
            MessageBox.Show("Customer added successfully!", "Add Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAllCustomers();
            ClearForm();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomerList.SelectedItem is not Customer selected) return;
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill all the fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string namePattern = @"^([\p{L}]+[\s]+){1,}[\p{L}]+$";
            if (!Regex.IsMatch(txtFullName.Text.Trim(), namePattern))
            {
                MessageBox.Show("Full name must contain at least two words and only letters (Vietnamese supported).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string phonePattern = @"^0\d{9}$";
            if (!Regex.IsMatch(txtPhone.Text.Trim(), phonePattern))
            {
                MessageBox.Show("Phone number is invalid. It should be 10 digits and start with 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(txtEmail.Text.Trim(), emailPattern))
            {
                MessageBox.Show("Email format is invalid.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var customer = con.Customers.FirstOrDefault(c => c.CustomerId == selected.CustomerId);
            if (customer == null)
            {
                MessageBox.Show("Customer not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            customer.FullName = txtFullName.Text;
            customer.Phone = txtPhone.Text;
            customer.Email = txtEmail.Text;
            customer.Address = txtAddress.Text;

            con.SaveChanges();
            MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAllCustomers();
            ClearForm();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomerList.SelectedItem is not Customer selected)
            {
                MessageBox.Show("Please select a customer to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool hasRentals = con.Rentals.Any(r => r.CustomerId == selected.CustomerId);

            if (hasRentals)
            {
                MessageBox.Show("This customer has rental history and cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this customer?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                con.Customers.Remove(selected);
                con.SaveChanges();
                MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllCustomers();
                ClearForm();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            dgCustomerList.SelectedItem = null;
            txtCustomerId.Text = "";
            txtFullName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
        }
    }
}
