using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UchebnayaPraktika
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            // Защита от падения при инициализации
            if (DgAuthorBooks == null || Core.Context == null || Core.CurrentUser == null) return;

            // Принудительно обновляем кэш EF
            foreach (var entry in Core.Context.ChangeTracker.Entries().ToList())
            {
                entry.Reload();
            }

            DgAuthorBooks.ItemsSource = Core.Context.Books
                .Where(b => b.AuthorId == Core.CurrentUser.Id)
                .ToList();
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditBookPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Books book)
            {
                NavigationService?.Navigate(new AddEditBookPage(book));
            }
        }

        private void BtnAppealBook_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Books book)
            {
                // Проверяем, нет ли уже активной заявки
                bool alreadyAppealed = Core.Context.UnfreezeRequests
                    .Any(r => r.BookId == book.Id && r.UserId == Core.CurrentUser.Id);

                if (alreadyAppealed)
                {
                    MessageBox.Show("Апелляция на эту книгу уже находится на рассмотрении.", "Внимание");
                    return;
                }

                UnfreezeRequests req = new UnfreezeRequests 
                { 
                    UserId = Core.CurrentUser.Id, 
                    BookId = book.Id, 
                    CreatedAt = DateTime.Now,
                    Reason = "Оспаривание заморозки автором" 
                };

                Core.Context.UnfreezeRequests.Add(req);
                Core.Context.SaveChanges();
                MessageBox.Show("Апелляция на книгу успешно отправлена.", "Успех");
            }
        }
    }
}