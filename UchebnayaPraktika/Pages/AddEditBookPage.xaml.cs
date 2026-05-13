using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UchebnayaPraktika
{
    public partial class AddEditBookPage : Page
    {
        private Books _currentBook;
        private string _selectedFilePath = null;

        public AddEditBookPage(Books book)
        {
            InitializeComponent();
            _currentBook = book ?? new Books();
            DataContext = _currentBook;

            if (book != null)
            {
                TbTitle.Text = book.Title;
                TbContent.Text = book.Content;
                if (!string.IsNullOrEmpty(book.CoverPath))
                {
                    ImgPreview.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + book.CoverPath));
                }
            }
        }

        private void BtnSelectCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png" };
            if (op.ShowDialog() == true)
            {
                _selectedFilePath = op.FileName;
                ImgPreview.Source = new BitmapImage(new Uri(_selectedFilePath));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _currentBook.Title = TbTitle.Text;
            _currentBook.Content = TbContent.Text;
            _currentBook.AuthorId = Core.CurrentUser.Id;

            // Если выбрали новое фото — сохраняем через наш FileManager
            if (_selectedFilePath != null)
            {
                _currentBook.CoverPath = FileManager.SaveImage(_selectedFilePath, "Covers");
            }

            if (_currentBook.Id == 0) Core.Context.Books.Add(_currentBook);

            Core.Context.SaveChanges();
            MessageBox.Show("Данные сохранены!");
            NavigationService.GoBack();
        }
    }
}