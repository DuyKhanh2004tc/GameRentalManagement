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
                {;
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
                    Status = false,
                };
                con.Games.Add(newGame);
                con.SaveChanges();
                MessageBox.Show("New game added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllGames();

            }
            catch (Exception)
            {

               
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
