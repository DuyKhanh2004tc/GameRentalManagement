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
                decimal baseTotal = rentalDetails.Sum(rd => rd.Quantity * rd.PriceAtRent);
                decimal overdueFee = 0;
                decimal total = baseTotal;

                // neu da tra hien thi total price da luu
                if (selected.Status == "Returned" && selected.TotalPrice.HasValue)
                {
                    total = selected.TotalPrice.Value;
                }
                else if (selected.ReturnDate == null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    if (selected.DueDate < today)
                    {
                        int overdueDays = today.DayNumber - selected.DueDate.DayNumber;
                        overdueFee = overdueDays * 5000;
                        total += overdueFee;
                    }
                }

                txtTotalAmount.Text = $"{baseTotal} VND";
                if (overdueFee > 0)
                {
                    txtTotalAmount.Text += $" (+ {overdueFee} VND overdue fee)";
                }
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
        private void btn_Returned_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgRentalList.SelectedItem as Rental;
            if (selected != null && selected.Status == "Returned")
            {
                MessageBox.Show("This rental has already been returned.");
                return;
            }

            if (selected != null && selected.ReturnDate == null)
            {
                selected.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
                selected.Status = "Returned";

                var rentalDetails = con.RentalDetails.Include(rd => rd.Game)
                                                     .Where(rd => rd.RentalId == selected.RentalId)
                                                     .ToList();

                decimal total = rentalDetails.Sum(rd => rd.Quantity * rd.PriceAtRent);

                // tinh them phi neu qua han
                if (selected.ReturnDate > selected.DueDate)
                {
                    int overdueDays = selected.ReturnDate.Value.DayNumber - selected.DueDate.DayNumber;
                    decimal overdueFee = overdueDays * 5000;
                    total += overdueFee;
                }

                selected.TotalPrice = total;

                foreach (var detail in rentalDetails)
                {
                    var game = con.Games.FirstOrDefault(g => g.GameId == detail.GameId);
                    if (game != null)
                    {
                        game.Quantity += detail.Quantity;
                    }
                }

                con.SaveChanges();
                LoadAllRental();
            }
            else
            {
                MessageBox.Show("Please select a valid rental to mark as returned.");
            }
        }
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgRentalList.SelectedItem as Rental;
            if (selected != null && selected.Status == "Returned")
            {
                MessageBox.Show("Cannot cancel a rental that has already been returned.");
                return;
            }
            if (selected != null)
            {
                var rentalDetails = con.RentalDetails.Where(rd => rd.RentalId == selected.RentalId).ToList();
                var result = MessageBox.Show("Are you sure to cancel this rental? ", "Confirm cancel", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var detail in rentalDetails)
                    {
                        var game = con.Games.FirstOrDefault(g => g.GameId == detail.GameId);
                        if (game != null)
                        {
                            game.Quantity += detail.Quantity;
                        }
                        con.RentalDetails.Remove(detail);
                    }
                    con.Rentals.Remove(selected);
                    con.SaveChanges();
                    LoadAllRental();
                }
                
            }
            else
            {
                MessageBox.Show("Please select a rental to delete.");
            }
        }
        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            ClearRentalInfor();
            LoadAllRental();
        }
        private void ClearRentalInfor()
        {
            txtRentalId.Text = string.Empty;
            txtCustomerId.Text = string.Empty;
            txtRentalDate.Text = string.Empty;
            txtDueDate.Text = string.Empty;
            txtReturnDate.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtProcessedBy.Text = string.Empty;
            dgRentalDetails.ItemsSource = null;
            txtTotalAmount.Text = string.Empty;
        }
    }
}
