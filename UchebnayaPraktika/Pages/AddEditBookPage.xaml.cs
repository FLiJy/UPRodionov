using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (book == null)
            {
                // Если это новая книга
                _currentBook = new Books();
                _currentBook.IsFrozen = false; // Явно делаем её активной (не замороженной)
            }
            else
            {
                // Если редактируем существующую
                _currentBook = book;
            }

            DataContext = _currentBook;

            // Загрузка жанров в ComboBox (из предыдущего шага)
            ComboGenres.ItemsSource = Core.Context.Genres.ToList();

            if (book != null)
            {
                TbTitle.Text = book.Title;
                TbContent.Text = book.Content;

                // Установка текущего жанра в ComboBox
                var currentGenre = book.BookGenres.FirstOrDefault()?.Genres;
                if (currentGenre != null)
                {
                    ComboGenres.SelectedItem = currentGenre;
                }

                if (!string.IsNullOrEmpty(book.CoverPath))
                {
                    try
                    {
                        ImgPreview.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + book.CoverPath));
                    }
                    catch { }
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
            // Проверка заполнения
            if (string.IsNullOrWhiteSpace(TbTitle.Text) || ComboGenres.SelectedItem == null)
            {
                MessageBox.Show("Заполните название и выберите жанр!");
                return;
            }

            _currentBook.Title = TbTitle.Text;
            _currentBook.Content = TbContent.Text;
            _currentBook.AuthorId = Core.CurrentUser.Id;

            // Сохранение обложки
            if (_selectedFilePath != null)
            {
                _currentBook.CoverPath = FileManager.SaveImage(_selectedFilePath, "Covers");
            }

            // Если это новая книга
            if (_currentBook.Id == 0)
            {
                _currentBook.IsFrozen = false; // Гарантируем статус "Активна"
                Core.Context.Books.Add(_currentBook);
            }

            // Логика сохранения жанров (как делали раньше)
            var selectedGenre = ComboGenres.SelectedItem as Genres;
            var existingGenres = Core.Context.BookGenres.Where(bg => bg.BookId == _currentBook.Id).ToList();
            if (existingGenres.Any()) Core.Context.BookGenres.RemoveRange(existingGenres);

            if (_currentBook.BookGenres == null) _currentBook.BookGenres = new List<BookGenres>();
            _currentBook.BookGenres.Add(new BookGenres { Genres = selectedGenre, Books = _currentBook });

            try
            {
                Core.Context.SaveChanges();
                MessageBox.Show("Книга успешно опубликована и сохранена!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}