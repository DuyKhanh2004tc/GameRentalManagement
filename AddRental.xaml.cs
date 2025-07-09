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
    /// Interaction logic for AddRental.xaml
    /// </summary>
    public partial class AddRental : Window
    {
        GameRentalDbContext con = new GameRentalDbContext();
        private int processedByUserId;
        public AddRental(int userId)
        {

            InitializeComponent();
            processedByUserId = userId;
            LoadData();
        }
        private void LoadData()
        {
            cbCustomer.ItemsSource = con.Customers.ToList();

            cbGame.ItemsSource = con.Games.Where(g => g.Quantity > 0).ToList();

            dpDueDate.DisplayDateStart = DateTime.Today;

        }
        private void btn_Finish_Click(object sender, RoutedEventArgs e)
        {
            int customerId = (int)cbCustomer.SelectedValue;
            int gameId = (int)cbGame.SelectedValue;
            var game = con.Games.FirstOrDefault( g => g.GameId == gameId);

            if(game==null|| game.Quantity <= 0)
            {
                MessageBox.Show("Selected Game is not available");
                return;
            }
            
            
            Rental rental = new Rental
            {
                CustomerId = customerId,
                RentalDate = DateOnly.FromDateTime(DateTime.Now),
                DueDate = DateOnly.FromDateTime(dpDueDate.SelectedDate.Value),
                ReturnDate = null,
                Status = "Borrowed",
                ProcessedBy = processedByUserId
            };
            con.Rentals.Add(rental);
            con.SaveChanges(); 

            
            RentalDetail rentalDetail = new RentalDetail
            {
                RentalId = rental.RentalId,
                GameId = gameId,
                Quantity = 1,
                PriceAtRent = game.PricePerDay
            };
            con.RentalDetails.Add(rentalDetail);
            game.Quantity--;
           
            con.SaveChanges();
            MessageBox.Show("Rental created successfully!");
            this.DialogResult = true;
            this.Close();
        }
    }
}
