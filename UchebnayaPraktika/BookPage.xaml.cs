using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class BookPage : Page
    {
        private Books _currentBook;

        // Переменные для отслеживания, на что именно подается жалоба в Overlay-панели
        private string _complaintType = ""; // "Book", "Author", "Review"
        private int? _targetReviewId = null;

        public BookPage(Books book)
        {
            InitializeComponent();
            _currentBook = book;

            LoadBookData();
            LoadReviews();
            CheckAdminRole();
        }

        // DTO-класс для вывода отзывов с учетом видимости кнопок администратора
        public class ReviewItem
        {
            public int Id { get; set; }
            public string AuthorName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
            public DateTime? CreatedAt { get; set; }
            public Visibility AdminButtonVisibility { get; set; }
        }

        private void CheckAdminRole()
        {
            if (Core.CurrentUser != null && Core.CurrentUser.Roles?.Name == "Admin")
            {
                BtnFreezeBook.Visibility = Visibility.Visible;
            }
        }

        private void LoadBookData()
        {
            // Обновляем сущность из контекста на случай изменений
            _currentBook = Core.Context.Books.FirstOrDefault(b => b.Id == _currentBook.Id);
            if (_currentBook == null) return;

            TbTitle.Text = _currentBook.Title;
            // Замените .Description на ваше поле из БД, если оно называется иначе
            TbDescription.Text = _currentBook.Description ?? "Описание отсутствует.";
            TbAuthor.Text = _currentBook.Users?.DisplayName ?? "Неизвестный автор";

            // Предполагается, что текст книги хранится в поле TextContent или аналогичном
            // Если текст хранится в файле, здесь должна быть логика считывания файла
            TbBookText.Text = "Здесь отображается полный текст произведения...\n\n" + (_currentBook.Description ?? "");

            // Получаем жанры через промежуточную таблицу BookGenres
            var genres = _currentBook.BookGenres.Select(bg => bg.Genres.Name).ToList();
            TbGenres.Text = genres.Any() ? string.Join(", ", genres) : "Не указаны";

            // Считаем рейтинг
            if (_currentBook.Reviews.Any())
            {
                TbRating.Text = _currentBook.Reviews.Average(r => r.Rating).ToString("F1");
            }
            else
            {
                TbRating.Text = "0.0";
            }
        }

        private void LoadReviews()
        {
            bool isAdmin = Core.CurrentUser != null && Core.CurrentUser.Roles?.Name == "Admin";

            // Замените .Text и .CreatedAt на ваши названия полей из таблицы Reviews, если они отличаются
            var reviews = Core.Context.Reviews
                .Where(r => r.BookId == _currentBook.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList()
                .Select(r => new ReviewItem
                {
                    Id = r.Id,
                    AuthorName = r.Users?.DisplayName ?? "Пользователь",
                    Rating = r.Rating,
                    Comment = r.Text, // Поле текста отзыва
                    CreatedAt = r.CreatedAt,
                    AdminButtonVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed
                }).ToList();

            IcReviews.ItemsSource = reviews;
        }

        // --- ЧТЕНИЕ КНИГИ ---
        private void BtnRead_Click(object sender, RoutedEventArgs e) => PanelReading.Visibility = Visibility.Visible;
        private void BtnCloseReading_Click(object sender, RoutedEventArgs e) => PanelReading.Visibility = Visibility.Collapsed;

        // --- ДОБАВЛЕНИЕ ОТЗЫВА ---
        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Только авторизованные пользователи могут оставлять отзывы.");
                return;
            }

            string text = TbNewReviewText.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Напишите текст отзыва.");
                return;
            }

            // Парсим оценку (от 1 до 10 согласно ограничению БД)
            int rating = CbNewRating.SelectedIndex + 1;

            // Проверяем, не оставлял ли пользователь уже отзыв
            var existingReview = Core.Context.Reviews.FirstOrDefault(r => r.BookId == _currentBook.Id && r.UserId == Core.CurrentUser.Id);
            if (existingReview != null)
            {
                MessageBox.Show("Вы уже оставляли отзыв на эту книгу.");
                return;
            }

            Reviews newReview = new Reviews
            {
                BookId = _currentBook.Id,
                UserId = Core.CurrentUser.Id,
                Rating = rating,
                Text = text, // Замените на ваше поле текста
                CreatedAt = DateTime.Now
            };

            Core.Context.Reviews.Add(newReview);
            Core.Context.SaveChanges();

            TbNewReviewText.Clear();
            LoadBookData(); // Обновит средний рейтинг
            LoadReviews();  // Обновит список
            MessageBox.Show("Отзыв успешно добавлен!");
        }

        // --- СИСТЕМА ЖАЛОБ (ОТКРЫТИЕ ПАНЕЛИ) ---
        private void BtnComplainBook_Click(object sender, RoutedEventArgs e)
        {
            _complaintType = "Book";
            TbComplaintTitle.Text = "Жалоба на книгу";
            OverlayComplaint.Visibility = Visibility.Visible;
        }

        private void BtnComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            _complaintType = "Author";
            TbComplaintTitle.Text = "Жалоба на автора";
            OverlayComplaint.Visibility = Visibility.Visible;
        }

        private void BtnComplainReview_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                _complaintType = "Review";
                _targetReviewId = (int)btn.Tag;
                TbComplaintTitle.Text = "Жалоба на отзыв";
                OverlayComplaint.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancelComplaint_Click(object sender, RoutedEventArgs e)
        {
            OverlayComplaint.Visibility = Visibility.Collapsed;
            TbComplaintReason.Clear();
            _targetReviewId = null;
        }

        // --- ОТПРАВКА ЖАЛОБЫ В БД ---
        private void BtnSubmitComplaint_Click(object sender, RoutedEventArgs e)
        {
            string reason = TbComplaintReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Опишите причину жалобы.");
                return;
            }

            Complaints newComplaint = new Complaints
            {
                UserId = Core.CurrentUser.Id,
                CreatedAt = DateTime.Now
                // Поле Reason/Message добавьте сюда, если оно есть в вашей таблице Complaints:
                // Reason = reason 
            };

            if (_complaintType == "Book")
            {
                newComplaint.BookId = _currentBook.Id;
            }
            else if (_complaintType == "Author")
            {
                // Привязываем к книге, но помечаем в тексте, что это на автора
                newComplaint.BookId = _currentBook.Id;
                // Если есть поле текста жалобы, раскомментируйте:
                // newComplaint.Reason = "НА АВТОРА: " + reason;
            }
            else if (_complaintType == "Review" && _targetReviewId.HasValue)
            {
                newComplaint.ReviewId = _targetReviewId.Value;
            }

            Core.Context.Complaints.Add(newComplaint);
            Core.Context.SaveChanges();

            BtnCancelComplaint_Click(null, null); // Скрываем и очищаем панель
            MessageBox.Show("Жалоба отправлена на рассмотрение администрации.");
        }

        // --- ФУНКЦИИ АДМИНИСТРАТОРА ---
        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите заморозить эту книгу? Она пропадет из общего каталога.", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _currentBook.IsFrozen = true;
                Core.Context.SaveChanges();
                MessageBox.Show("Книга заморожена.");

                // Возвращаемся в каталог
                if (this.Parent is Frame frame)
                {
                    frame.Navigate(new CatalogPage());
                }
            }
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int reviewId = (int)btn.Tag;
                var review = Core.Context.Reviews.FirstOrDefault(r => r.Id == reviewId);

                if (review != null && MessageBox.Show("Заморозить/удалить этот отзыв?", "Модерация", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // ПРИМЕЧАНИЕ: В SQL скрипте у таблицы Reviews нет поля IsFrozen.
                    // Если у вас в модели оно создано вручную, используйте: review.IsFrozen = true;
                    // Иначе просто удаляем отзыв из БД:
                    Core.Context.Reviews.Remove(review);

                    Core.Context.SaveChanges();
                    LoadBookData();
                    LoadReviews();
                }
            }
        }
    }
}