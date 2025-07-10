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
using GameRentalManagement.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore;

namespace GameRentalManagement.UserControls
{
    /// <summary>
    /// Interaction logic for RentalPage.xaml
    /// </summary>
    public partial class RentalPage : UserControl
    {
        GameRentalDbContext con = new GameRentalDbContext();
        private int curUserId;
        public RentalPage(int userId)
        {
            InitializeComponent();
            curUserId = userId;
            LoadAllRental();
        }
        private void LoadAllRental()
        {
            var rentals = con.Rentals.Include(r => r.Customer).Include(r => r.ProcessedByNavigation).ToList();
            foreach (var rental in rentals)
            {
                if (rental.ReturnDate == null && rental.DueDate < DateOnly.FromDateTime(DateTime.Now) && rental.Status != "Overdue")
                {
                    rental.Status = "Overdue";
                    con.SaveChanges();
                }
            }

            dgRentalList.ItemsSource = rentals;
        }
        private void dgRentalList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = dgRentalList.SelectedItem as Rental;
            if (selected != null)
            {
                txtRentalId.Text = selected.RentalId.ToString();
                txtCustomerId.Text = selected.CustomerId.ToString();
                txtRentalDate.Text = selected.RentalDate.ToString();
                txtDueDate.Text = selected.DueDate.ToString();
                txtReturnDate.Text = selected.ReturnDate.ToString();
                txtStatus.Text = selected.Status;
                txtProcessedBy.Text = selected.UserName.ToString();
                var rentalDetails = con.RentalDetails.Include(rd => rd.Game).Where(rd => rd.RentalId == selected.RentalId).ToList();

                dgRentalDetails.ItemsSource = rentalDetails;
                decimal total = rentalDetails.Sum(rd => rd.Quantity * rd.PriceAtRent);
                txtTotalAmount.Text = $"{total}";
            }

        }
        private void btn_newRental_Click(object sender, RoutedEventArgs e)
        {
            var addRentalWindow = new AddRental(curUserId);
            bool? result = addRentalWindow.ShowDialog();
            if (result == true)
            {
                LoadAllRental(); 
            }
        }
    }
}
