using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = Core.CurrentUser;
            if (user == null) return;

            TbDisplayName.Text = user.DisplayName;
            TbLogin.Text = user.Login;
            TbEmail.Text = user.Email;
            TbRole.Text = user.Roles?.Name ?? "Пользователь";

            // Проверка заморозки
            if (user.IsFrozen == true)
            {
                BorderFrozen.Visibility = Visibility.Visible;
            }

            // Кнопка автора (показываем только обычным пользователям)
            if (user.Roles?.Name == "User")
            {
                var request = Core.Context.RoleRequests.FirstOrDefault(r => r.UserId == user.Id && r.Status == "Pending");
                if (request != null) TbAuthorStatus.Visibility = Visibility.Visible;
                else BtnRequestAuthor.Visibility = Visibility.Visible;
            }

            // Загрузка отзывов
            IcMyReviews.ItemsSource = Core.Context.Reviews.Where(r => r.UserId == user.Id).ToList();
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            RoleRequests req = new RoleRequests { UserId = Core.CurrentUser.Id, Status = "Pending", CreatedAt = DateTime.Now };
            Core.Context.RoleRequests.Add(req);
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка отправлена!");
            LoadUserData();
        }

        private void BtnAppealAccount_Click(object sender, RoutedEventArgs e)
        {
            // Упрощенная логика подачи апелляции
            UnfreezeRequests req = new UnfreezeRequests { UserId = Core.CurrentUser.Id, CreatedAt = DateTime.Now };
            Core.Context.UnfreezeRequests.Add(req);
            Core.Context.SaveChanges();
            MessageBox.Show("Запрос на разморозку отправлен.");
        }
    }
}