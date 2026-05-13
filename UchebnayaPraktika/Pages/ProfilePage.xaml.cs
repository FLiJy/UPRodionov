using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UchebnayaPraktika
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            Loaded += ProfilePage_Loaded;
        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Context.Entry(Core.CurrentUser).Reload();
            DataContext = Core.CurrentUser;

          
        }

        

        private void LoadUserData()
        {
            var user = Core.CurrentUser;
            TbDisplayName.Text = user.DisplayName;

            if (!string.IsNullOrEmpty(user.PhotoPath))
            {
                try
                {
                    // Собираем путь для локального файла
                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, user.PhotoPath.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ImgAvatar.ImageSource = new BitmapImage(new Uri(fullPath));
                    }
                }
                catch { }
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png" };
            if (op.ShowDialog() == true)
            {
                string path = FileManager.SaveImage(op.FileName, "Avatars");
                if (path != null)
                {
                    Core.CurrentUser.PhotoPath = path;
                    ImgAvatar.ImageSource = new BitmapImage(new Uri(op.FileName));
                }
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            Core.CurrentUser.DisplayName = TbDisplayName.Text;
            Core.Context.SaveChanges();
            MessageBox.Show("Профиль обновлен!");
        }

        private void BtnAppeal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем запись в таблице жалоб
                var appeal = new UnfreezeRequests
                {
                    UserId = Core.CurrentUser.Id,
                    Reason = "Апелляция на разморозку аккаунта",
                    CreatedAt = DateTime.Now,

                };

                Core.Context.UnfreezeRequests.Add(appeal);
                Core.Context.SaveChanges();

                MessageBox.Show("Ваша апелляция отправлена администратору.");
                BtnAppeal.IsEnabled = false;
                BtnAppeal.Content = "Отправлено";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}