using System.Linq;
using System.Windows;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1. Простая валидация
            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(PbPassword.Password))
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            if (PbPassword.Password != PbConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            // 2. Проверка уникальности логина/email
            if (Core.Context.Users.Any(u => u.Login == TbLogin.Text || u.Email == TbEmail.Text))
            {
                MessageBox.Show("Пользователь с таким логином или Email уже существует.");
                return;
            }

            // 3. Создание пользователя
            // Находим роль по умолчанию (например, "User" или ID = 1)
            var defaultRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "User")
                              ?? Core.Context.Roles.FirstOrDefault();

            Users newUser = new Users
            {
                DisplayName = TbName.Text,
                Login = TbLogin.Text,
                Email = TbEmail.Text,
                Password = PbPassword.Password,
                RoleId = defaultRole?.Id ?? 1,
                IsFrozen = false
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            MessageBox.Show("Регистрация успешна! Теперь вы можете войти.");

            BtnBack_Click(null, null);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow auth = new AuthWindow();
            auth.Show();
            this.Close();
        }
    }
}