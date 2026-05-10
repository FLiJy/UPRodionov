using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UchebnayaPraktika
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            if (Core.Context == null) return;

            // Принудительно обновляем кэш EF, чтобы видеть свежие жалобы и заявки
            foreach (var entry in Core.Context.ChangeTracker.Entries().ToList())
            {
                entry.Reload();
            }

            DgComplaints.ItemsSource = Core.Context.Complaints.ToList();
            DgRoleRequests.ItemsSource = Core.Context.RoleRequests.Where(r => r.Status == "Pending").ToList();
            DgUnfreeze.ItemsSource = Core.Context.UnfreezeRequests.ToList();
            DgUsers.ItemsSource = Core.Context.Users.ToList();
        }

        // --- ЗАЯВКИ НА АВТОРА ---
        private void BtnApproveAuthor_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is RoleRequests req)
            {
                req.Status = "Approved";

                // Ищем ID роли "Author". Если не найдет, ставим 2 по умолчанию
                var authorRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "Author");
                if (authorRole != null)
                {
                    req.Users.RoleId = authorRole.Id;
                }

                Core.Context.SaveChanges();
                LoadAllData();
            }
        }

        private void BtnRejectAuthor_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is RoleRequests req)
            {
                req.Status = "Rejected";
                Core.Context.SaveChanges();
                LoadAllData();
            }
        }

        // --- ЖАЛОБЫ ---
        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Complaints complaint)
            {
                Core.Context.Complaints.Remove(complaint);
                Core.Context.SaveChanges();
                LoadAllData();
            }
        }

        private void BtnFreezeTarget_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Complaints complaint)
            {
                // Если жалоба на книгу - морозим книгу
                if (complaint.BookId != null && complaint.Books != null)
                {
                    complaint.Books.IsFrozen = true;
                }

                // Если жалоба на отзыв - удаляем отзыв (т.к. поля IsFrozen у него нет в БД)
                if (complaint.ReviewId != null && complaint.Reviews != null)
                {
                    Core.Context.Reviews.Remove(complaint.Reviews);
                }

                // Удаляем саму обработанную жалобу
                Core.Context.Complaints.Remove(complaint);
                Core.Context.SaveChanges();

                LoadAllData();
                MessageBox.Show("Меры приняты. Объект заморожен или удален.", "Успешно");
            }
        }

        // --- РАЗМОРОЗКА ---
        private void BtnUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UnfreezeRequests req)
            {
                if (req.BookId != null && req.Books != null)
                {
                    req.Books.IsFrozen = false;
                }

                if (req.UserId != null && req.Users != null)
                {
                    req.Users.IsFrozen = false;
                }

                Core.Context.UnfreezeRequests.Remove(req);
                Core.Context.SaveChanges();

                LoadAllData();
                MessageBox.Show("Объект успешно разморожен!", "Успешно");
            }
        }

        // --- ПОЛЬЗОВАТЕЛИ ---
        private void BtnResetPass_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Users user)
            {
                user.Password = "123"; // Простой сброс пароля
                Core.Context.SaveChanges();
                MessageBox.Show($"Пароль пользователя {user.Login} сброшен на '123'", "Сброс пароля");
            }
        }

        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Users user)
            {
                var userRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "User");
                var authorRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "Author");

                if (userRole != null && authorRole != null)
                {
                    // Меняем туда-обратно
                    user.RoleId = (user.RoleId == userRole.Id) ? authorRole.Id : userRole.Id;
                    Core.Context.SaveChanges();
                    LoadAllData();
                }
            }
        }

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Users user)
            {
                user.IsFrozen = true;
                Core.Context.SaveChanges();
                LoadAllData();
            }
        }
    }
}