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

        // Lưu gameId -> quantity đã chọn
        private Dictionary<int, int> selectedGameQuantities = new Dictionary<int, int>();

        public AddRental(int userId)
        {
            InitializeComponent();
            processedByUserId = userId;
            LoadData();
        }

        private void LoadData()
        {
            cbCustomer.ItemsSource = con.Customers.ToList();
            cbCustomer.DisplayMemberPath = "FullName";
            cbCustomer.SelectedValuePath = "CustomerId";

            dgGameList.ItemsSource = con.Games.Where(g => g.Quantity > 0).ToList();
            dpDueDate.DisplayDateStart = DateTime.Today;
        }

        private void btn_Finish_Click(object sender, RoutedEventArgs e)
        {
            if (cbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer.");
                return;
            }

            int customerId = (int)cbCustomer.SelectedValue;

            if (selectedGameQuantities.Count == 0)
            {
                MessageBox.Show("Please select at least one game and enter quantity.");
                return;
            }

            // Kiểm tra hợp lệ: số lượng nhập vào phải > 0 và nhỏ hơn hoặc bằng tồn kho
            foreach (var kvp in selectedGameQuantities)
            {
                int gameId = kvp.Key;
                int quantity = kvp.Value;

                if (quantity <= 0)
                {
                    MessageBox.Show($"Invalid quantity for game ID {gameId}.");
                    return;
                }

                var game = con.Games.FirstOrDefault(g => g.GameId == gameId);
                if (game == null || game.Quantity < quantity)
                {
                    MessageBox.Show($"Not enough stock for game '{game?.GameName}' (available: {game?.Quantity}).");
                    return;
                }
            }

            // Tạo đơn mượn
            var rental = new Rental
            {
                CustomerId = customerId,
                RentalDate = DateOnly.FromDateTime(DateTime.Now),
                DueDate = DateOnly.FromDateTime(dpDueDate.SelectedDate ?? DateTime.Today),
                ReturnDate = null,
                Status = "Borrowed",
                ProcessedBy = processedByUserId
            };
            con.Rentals.Add(rental);
            con.SaveChanges(); // để lấy rental.RentalId

            // Tạo chi tiết cho từng game đã chọn
            foreach (var kvp in selectedGameQuantities)
            {
                int gameId = kvp.Key;
                int quantity = kvp.Value;

                var game = con.Games.First(g => g.GameId == gameId);

                var rentalDetail = new RentalDetail
                {
                    RentalId = rental.RentalId,
                    GameId = gameId,
                    Quantity = quantity,
                    PriceAtRent = game.PricePerDay
                };
                con.RentalDetails.Add(rentalDetail);

                game.Quantity -= quantity;
            }

            con.SaveChanges();

            MessageBox.Show("Rental created successfully!");
            this.DialogResult = true;
            this.Close();
        }

        // Khi người dùng tick chọn game
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox?.Tag == null) return;

            int gameId = Convert.ToInt32(checkbox.Tag);
            if (!selectedGameQuantities.ContainsKey(gameId))
            {
                selectedGameQuantities[gameId] = 1; // mặc định
            }
        }

        // Khi người dùng bỏ chọn
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox?.Tag == null) return;

            int gameId = Convert.ToInt32(checkbox.Tag);
            if (selectedGameQuantities.ContainsKey(gameId))
            {
                selectedGameQuantities.Remove(gameId);
            }
        }

        // Khi người dùng nhập số lượng
        private void QuantityBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox?.Tag == null) return;

            int gameId = Convert.ToInt32(textbox.Tag);

            if (int.TryParse(textbox.Text, out int quantity))
            {
                if (selectedGameQuantities.ContainsKey(gameId))
                {
                    selectedGameQuantities[gameId] = quantity;
                }
            }
        }
    }
}
