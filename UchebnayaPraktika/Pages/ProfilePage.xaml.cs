using Microsoft.Win32;
using System;
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
            Loaded += ProfilePage_Loaded; // Событие при каждом открытии страницы
    }

    private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        // 1. Принудительно обновляем данные текущего юзера из БД
        Core.Context.Entry(Core.CurrentUser).Reload();

        // 2. Устанавливаем DataContext страницы, чтобы XAML увидел статус IsFrozen
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
                    ImgAvatar.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + user.PhotoPath));
                }
                catch { /* игнорируем ошибку если файл удален */ }
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png" };
            if (op.ShowDialog() == true)
            {
                // Сразу сохраняем файл и обновляем путь в текущем юзере
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
    }
}