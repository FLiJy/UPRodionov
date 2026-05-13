using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UchebnayaPraktika
{
    public partial class BookPage : Page
    {
        private Books _currentBook;
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

        public class ReviewItem
        {
            public int Id { get; set; }
            public string AuthorName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } // DTO свойство мапится на Text из БД
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
            if (Core.Context == null) return;

            _currentBook = Core.Context.Books.FirstOrDefault(b => b.Id == _currentBook.Id);
            if (_currentBook == null) return;

            TbTitle.Text = _currentBook.Title;
            TbDescription.Text = _currentBook.Description ?? "Описание отсутствует.";
            TbAuthor.Text = _currentBook.Users?.DisplayName ?? "Неизвестный автор";

            // Подтягиваем обложку, если путь указан
            if (!string.IsNullOrWhiteSpace(_currentBook.CoverPath))
            {
                try
                {
                    ImgBookCover.Source = new BitmapImage(new Uri(_currentBook.CoverPath, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    // Если файл удален или путь битый, оставляем пустое место
                    ImgBookCover.Source = null;
                }
            }

            // Читаем текст книги строго из поля Content
            TbBookText.Text = string.IsNullOrWhiteSpace(_currentBook.Content)
                ? "Текст произведения отсутствует."
                : _currentBook.Content;

            // Жанры
            if (_currentBook.BookGenres != null)
            {
                var genres = _currentBook.BookGenres.Select(bg => bg.Genres.Name).ToList();
                TbGenres.Text = genres.Any() ? string.Join(", ", genres) : "Не указаны";
            }

            // Рейтинг
            if (_currentBook.Reviews != null && _currentBook.Reviews.Any())
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
            if (IcReviews == null || Core.Context == null) return;

            bool isAdmin = Core.CurrentUser != null && Core.CurrentUser.Roles?.Name == "Admin";

            var reviews = Core.Context.Reviews
                .Where(r => r.BookId == _currentBook.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList()
                .Select(r => new ReviewItem
                {
                    Id = r.Id,
                    AuthorName = r.Users?.DisplayName ?? "Пользователь",
                    Rating = r.Rating,
                    Comment = r.Text, // Безопасный маппинг поля Text
                    CreatedAt = r.CreatedAt,
                    AdminButtonVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed
                }).ToList();

            IcReviews.ItemsSource = reviews;
        }

        // --- ЧТЕНИЕ КНИГИ ---
        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            if (PanelReading != null) PanelReading.Visibility = Visibility.Visible;
        }

        private void BtnCloseReading_Click(object sender, RoutedEventArgs e)
        {
            if (PanelReading != null) PanelReading.Visibility = Visibility.Collapsed;
        }

        // --- ДОБАВЛЕНИЕ ОТЗЫВА ---
        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Только авторизованные пользователи могут оставлять отзывы.", "Ошибка");
                return;
            }

            string text = TbNewReviewText.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Напишите текст отзыва.", "Внимание");
                return;
            }

            int rating = CbNewRating.SelectedIndex + 1;

            var existingReview = Core.Context.Reviews
                .FirstOrDefault(r => r.BookId == _currentBook.Id && r.UserId == Core.CurrentUser.Id);

            if (existingReview != null)
            {
                MessageBox.Show("Вы уже оставляли отзыв на эту книгу.", "Внимание");
                return;
            }

            Reviews newReview = new Reviews
            {
                BookId = _currentBook.Id,
                UserId = Core.CurrentUser.Id,
                Rating = rating,
                Text = text,
                CreatedAt = DateTime.Now
            };

            Core.Context.Reviews.Add(newReview);
            Core.Context.SaveChanges();

            TbNewReviewText.Clear();
            LoadBookData();
            LoadReviews();
            MessageBox.Show("Отзыв успешно добавлен!", "Успех");
        }

        // --- ЖАЛОБЫ (ОТКРЫТИЕ ПАНЕЛИ) ---
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
            if ((sender as Button)?.Tag is int revId)
            {
                _complaintType = "Review";
                _targetReviewId = revId;
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

        // --- ОТПРАВКА ЖАЛОБЫ ---
        private void BtnSubmitComplaint_Click(object sender, RoutedEventArgs e)
        {
            string reasonText = TbComplaintReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reasonText))
            {
                MessageBox.Show("Опишите причину жалобы.", "Внимание");
                return;
            }

            Complaints newComplaint = new Complaints
            {
                UserId = Core.CurrentUser.Id,
                CreatedAt = DateTime.Now,
                Reason = reasonText // Сохраняем в правильное поле БД
            };

            if (_complaintType == "Book")
            {
                newComplaint.BookId = _currentBook.Id;
            }
            else if (_complaintType == "Author")
            {
                newComplaint.BookId = _currentBook.Id;
                newComplaint.Reason = "НА АВТОРА: " + reasonText;
            }
            else if (_complaintType == "Review" && _targetReviewId.HasValue)
            {
                newComplaint.ReviewId = _targetReviewId.Value;
            }

            Core.Context.Complaints.Add(newComplaint);
            Core.Context.SaveChanges();

            BtnCancelComplaint_Click(null, null);
            MessageBox.Show("Жалоба отправлена на рассмотрение администрации.", "Успех");
        }

        // --- МОДЕРАЦИЯ ---
        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Заморозить эту книгу? Она пропадет из каталога.", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _currentBook.IsFrozen = true;
                Core.Context.SaveChanges();
                MessageBox.Show("Книга заморожена.", "Успех");

                if (this.Parent is Frame frame)
                {
                    frame.Navigate(new CatalogPage());
                }
            }
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int reviewId)
            {
                var review = Core.Context.Reviews.FirstOrDefault(r => r.Id == reviewId);

                if (review != null && MessageBox.Show("Удалить этот отзыв?", "Модерация", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Core.Context.Reviews.Remove(review);
                    Core.Context.SaveChanges();

                    LoadBookData();
                    LoadReviews();
                }
            }
        }
    }
}