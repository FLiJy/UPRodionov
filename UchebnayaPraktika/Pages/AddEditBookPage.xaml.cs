using Microsoft.Win32;
using System;
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

            _currentBook = book ?? new Books();
            DataContext = _currentBook;

            // 1. Загружаем все доступные жанры в выпадающий список
            ComboGenres.ItemsSource = Core.Context.Genres.ToList();

            if (book != null)
            {
                TbTitle.Text = book.Title;
                TbContent.Text = book.Content;

                // 2. Если у книги уже есть жанр, выбираем его в ComboBox
                // (Предполагаем связь многие-ко-многим через таблицу BookGenres)
                var currentGenre = book.BookGenres.FirstOrDefault()?.Genres;
                if (currentGenre != null)
                {
                    ComboGenres.SelectedItem = currentGenre;
                }

                if (!string.IsNullOrEmpty(book.CoverPath))
                {
                    try
                    {
                        string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.CoverPath.TrimStart('/', '\\'));
                        ImgPreview.Source = new BitmapImage(new Uri(fullPath));
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
            // Валидация
            if (string.IsNullOrWhiteSpace(TbTitle.Text) || ComboGenres.SelectedItem == null)
            {
                MessageBox.Show("Заполните название и выберите жанр!");
                return;
            }

            _currentBook.Title = TbTitle.Text;
            _currentBook.Content = TbContent.Text;
            _currentBook.AuthorId = Core.CurrentUser.Id;

            // Сохранение фото
            if (_selectedFilePath != null)
            {
                _currentBook.CoverPath = FileManager.SaveImage(_selectedFilePath, "Covers");
            }

            // 3. Сохранение жанра (обновляем таблицу связей BookGenres)
            var selectedGenre = ComboGenres.SelectedItem as Genres;

            // Если книга новая, создаем коллекцию связей
            if (_currentBook.BookGenres == null) _currentBook.BookGenres = new System.Collections.Generic.List<BookGenres>();

            // Удаляем старые жанры книги и добавляем новый выбранный
            var existingGenres = Core.Context.BookGenres.Where(bg => bg.BookId == _currentBook.Id).ToList();
            if (existingGenres.Any())
            {
                Core.Context.BookGenres.RemoveRange(existingGenres);
            }

            _currentBook.BookGenres.Add(new BookGenres
            {
                Genres = selectedGenre,
                Books = _currentBook
            });

            if (_currentBook.Id == 0)
                Core.Context.Books.Add(_currentBook);

            try
            {
                Core.Context.SaveChanges();
                MessageBox.Show("Данные сохранены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }
    }
}