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

namespace GameRentalManagement
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : UserControl
    {
        GameRentalDbContext con = new GameRentalDbContext();
        public GamePage()
        {
            InitializeComponent();
            LoadAllGames();
            ClearForm();
        }
        private void LoadAllGames()
        {
            var games = con.Games.ToList();
            dgGameList.ItemsSource = games;

        }
        private void FilterGames(string keywords)
        {
            var games = con.Games
                .Where(g => (g.Platform != null && g.Platform.ToLower().Contains(keywords)) ||
                            (g.GameName != null && g.GameName.ToLower().Contains(keywords)) ||
                            (g.PricePerDay != null && g.PricePerDay.ToString().Contains(keywords)) ||
                            (g.Genre != null && g.Genre.ToLower().Contains(keywords))).ToList();
            dgGameList.ItemsSource = games;
        }
        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keywords = txtFilter.Text.ToLower();
            FilterGames(keywords);
        }
        

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(con.Games.Any(g => g.GameName == txtGameName.Text))
                {
                    MessageBox.Show("The game with the same name existed." ,"Duplicate Name of Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if(string.IsNullOrWhiteSpace(txtGameName.Text) || 
                   string.IsNullOrWhiteSpace(txtPlatform.Text) ||
                   string.IsNullOrWhiteSpace(txtGenre.Text) ||
                   string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                   string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    MessageBox.Show("Please fill all of fields above, these fields must not be empty", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Game newGame = new Game
                {
                    GameName = txtGameName.Text,
                    Platform = txtPlatform.Text,
                    Genre = txtGenre.Text,
                    Quantity = int.Parse(txtQuantity.Text),
                    PricePerDay = decimal.Parse(txtPrice.Text),
                    Status = rbAvailable.IsChecked == true
                };
                con.Games.Add(newGame);
                con.SaveChanges();
                MessageBox.Show("New game added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllGames();
                ClearForm();

            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred while adding the game. Please check your input and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            
        }

        private void ClearForm()
        {
            txtGameId.Text = "";
            txtGameName.Text = "";
            txtPlatform.Text = "";
            txtGenre.Text = "";
            txtPrice.Text = "";
            txtQuantity.Text = "";
            rbAvailable.IsChecked = true;
            rbInactive.IsChecked = false;
        }
        private void dgGameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dgGameList.SelectedItem is Game selectedGame)
            {
                txtGameId.Text = selectedGame.GameId.ToString();
                txtGameName.Text = selectedGame.GameName;
                txtPlatform.Text = selectedGame.Platform;
                txtGenre.Text = selectedGame.Genre;
                txtQuantity.Text = selectedGame.Quantity.ToString();
                txtPrice.Text = selectedGame.PricePerDay.ToString();
                rbAvailable.IsChecked = selectedGame.Status == true;
                rbInactive.IsChecked = selectedGame.Status == false;

            }
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            if(dgGameList.SelectedItem is Game selectedGame)
            {
                id = selectedGame.GameId;
            }
            var game = con.Games.FirstOrDefault(g => g.GameId == id);
            if (game == null)
            {
                MessageBox.Show("Game not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (txtGameName.Text.Trim().ToLower() != game.GameName.Trim().ToLower())
            {
                if (con.Games.Any(g => g.GameId != id && g.GameName.Trim().ToLower() == txtGameName.Text.Trim().ToLower()))
                {
                    MessageBox.Show("The game with the same name existed.", "Duplicate Name of Game", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(txtGameName.Text) ||
                   string.IsNullOrWhiteSpace(txtPlatform.Text) ||
                   string.IsNullOrWhiteSpace(txtGenre.Text) ||
                   string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                   string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill all of fields above, these fields must not be empty", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            game.GameName = txtGameName.Text;
            game.Platform = txtPlatform.Text;
            game.Genre = txtGenre.Text;
            game.Quantity = int.Parse(txtQuantity.Text);
            game.PricePerDay = decimal.Parse(txtPrice.Text);
            game.Status = rbAvailable.IsChecked == true;
            con.SaveChanges();
            MessageBox.Show("This game updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAllGames();
            ClearForm();
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgGameList.SelectedItem is Game selectedGame)
            {
                try
                {
                    bool isCurrentlyRented = con.RentalDetails.Any(rd => rd.GameId == selectedGame.GameId && rd.Rental.Status == "Borrowed");
                    if (isCurrentlyRented)
                    {
                        MessageBox.Show("This game is currently rented out and cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool hasRentalHistory = con.RentalDetails.Any(rd => rd.GameId == selectedGame.GameId);
                    if (hasRentalHistory)
                    {
                        MessageBoxResult result = MessageBox.Show("This game has already been rented before. Are you sure to Delete this game? Rental records containing this game may be lost." +
                             "Click 'Yes' to delete the game or 'No' to mark it as inactive.", "Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            var rentalDetails = con.RentalDetails.Where(rd => rd.GameId == selectedGame.GameId).ToList();
                            con.RentalDetails.RemoveRange(rentalDetails);
                            con.Games.Remove(selectedGame);
                            con.SaveChanges();
                            MessageBox.Show("Game deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadAllGames();
                        }



                    }
                    else
                    {
                        con.Games.Remove(selectedGame);
                        con.SaveChanges();
                        MessageBox.Show("Game deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAllGames();
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a game to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            dgGameList.SelectedItem = null;
            ClearForm();
        }
    }
}
