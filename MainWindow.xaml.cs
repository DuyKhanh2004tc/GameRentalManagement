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

namespace GameRentalManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameRentalDbContext con = new GameRentalDbContext();
        private int currentUserId;
        private string currentRole;
        public MainWindow(int userId, string role)
        {
            currentUserId = userId;
            currentRole = role;
            InitializeComponent();
        }

        private void btn_Game_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new GamePage();
        }
    }
}