using System.Linq;
using System.Windows;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text;
            string password = PbPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            // Ищем пользователя в БД
            var user = Core.Context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                Core.CurrentUser = user;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Здесь переход на окно регистрации
            // RegistrationWindow reg = new RegistrationWindow();
            // reg.Show();
            // this.Close();
            MessageBox.Show("Здесь будет открываться окно регистрации.");
        }
    }
}