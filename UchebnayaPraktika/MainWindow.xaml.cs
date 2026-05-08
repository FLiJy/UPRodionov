using System.Windows;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupSidebar();
            // По умолчанию открываем каталог
            MainFrame.Navigate(new CatalogPage());
        }

        private void SetupSidebar()
        {
            if (Core.CurrentUser == null) return;

            // Предполагается, что в БД есть таблица Roles. 
            // Замените "Admin" и "Author" на точные названия ролей из вашей БД.
            string userRole = Core.CurrentUser.Roles?.Name;

            if (userRole == "Admin")
            {
                BtnAdmin.Visibility = Visibility.Visible;
            }
            if (userRole == "Author" || userRole == "Admin")
            {
                BtnAuthor.Visibility = Visibility.Visible;
            }

            // Если аккаунт заморожен, можно сразу перекинуть на страницу профиля или предупреждения
            if (Core.CurrentUser.IsFrozen == true)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Доступ ограничен.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                MainFrame.Navigate(new ProfilePage());
            }
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
        private void BtnLists_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ListsPage());
        private void BtnAuthor_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuthorPage());
        private void BtnAdmin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
        private void BtnProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Core.CurrentUser = null;
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();
            this.Close();
        }
    }
}