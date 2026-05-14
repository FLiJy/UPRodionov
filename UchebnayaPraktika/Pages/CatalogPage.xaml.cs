using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            UpdateBooks();
        }

        public class BookCard
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string CoverPath { get; set; }
            public string AuthorName { get; set; }
            public double AverageRating { get; set; }
            public Books OriginalBook { get; set; }
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.ToList();
            genres.Insert(0, new Genres { Id = 0, Name = "Все жанры" });
            CbGenres.ItemsSource = genres;
            CbGenres.SelectedIndex = 0;
        }

        private void UpdateBooks()
        {
            if (Core.Context == null) return;

            // Берем только незамороженные книги 
            var query = Core.Context.Books.Where(b => b.IsFrozen == false).AsQueryable();

            // 1.Поиск по названию или автору
            string searchText = TbSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(b => b.Title.ToLower().Contains(searchText) ||
                                         (b.Users != null && b.Users.DisplayName.ToLower().Contains(searchText)));
            }

            // 2.Фильтрация по жанру
            if (CbGenres.SelectedItem is Genres selectedGenre && selectedGenre.Id != 0)
            {
                query = query.Where(b => b.BookGenres.Any(bg => bg.GenreId == selectedGenre.Id));
            }

            var booksList = query.ToList();

            var bookCards = booksList.Select(b => new BookCard
            {
                Id = b.Id,
                Title = b.Title,
                CoverPath = b.CoverPath,
                AuthorName = b.Users?.DisplayName ?? "Неизвестный автор",
                OriginalBook = b,
                // Считаем среднюю оценку
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0
            }).ToList();

            // 3. Сортировка
            switch (CbSort.SelectedIndex)
            {
                case 0: // По названию (А-Я)
                    bookCards = bookCards.OrderBy(b => b.Title).ToList();
                    break;
                case 1: // По названию (Я-А)
                    bookCards = bookCards.OrderByDescending(b => b.Title).ToList();
                    break;
                case 2: // Высокая оценка
                    bookCards = bookCards.OrderByDescending(b => b.AverageRating).ToList();
                    break;
                case 3: // Низкая оценка
                    bookCards = bookCards.OrderBy(b => b.AverageRating).ToList();
                    break;
            }
            if (LvBooks == null)
            {
                return;
            }

            LvBooks.ItemsSource = bookCards;
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateBooks();
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateBooks();
        private void BtnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int bookId = (int)btn.Tag;
                var book = Core.Context.Books.FirstOrDefault(b => b.Id == bookId);
                if (book != null)
                {
                    NavigationService.Navigate(new BookPage(book));
                }
            }
        }

        // Добавление в списки 
        private void CbAddToList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || cb.SelectedIndex <= 0) return; 

            int bookId = (int)cb.Tag;
            string status = "";

            switch (cb.SelectedIndex)
            {
                case 1: status = "Planned"; break;   // В планах
                case 2: status = "Reading"; break;   // Читаю
                case 3: status = "Completed"; break; // Прочитано
                case 4: status = "Dropped"; break;   // Заброшено
            }

            if (Core.CurrentUser != null && !string.IsNullOrEmpty(status))
            {
                // Проверяем, есть ли уже эта книга в списках пользователя
                var existingEntry = Core.Context.ReadingLists.FirstOrDefault(rl => rl.UserId == Core.CurrentUser.Id && rl.BookId == bookId);

                if (existingEntry != null)
                {
                    existingEntry.Status = status; // Обновляем статус
                }
                else
                {
                    // Добавляем новую запись
                    ReadingLists newListEntry = new ReadingLists
                    {
                        UserId = Core.CurrentUser.Id,
                        BookId = bookId,
                        Status = status
                    };
                    Core.Context.ReadingLists.Add(newListEntry);
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Статус книги обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Сбрасываем индекс обратно на "В список...", чтобы можно было выбрать снова
                cb.SelectedIndex = 0;
            }
        }
    }
}