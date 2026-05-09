using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UchebnayaPraktika;

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
            DgAuthorBooks.ItemsSource = Core.Context.Books.Where(b => b.AuthorId == Core.CurrentUser.Id).ToList();
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditBookPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var book = (sender as Button).Tag as Books;
            NavigationService.Navigate(new AddEditBookPage(book));
        }

        private void BtnAppealBook_Click(object sender, RoutedEventArgs e)
        {
            var book = (sender as Button).Tag as Books;
            UnfreezeRequests req = new UnfreezeRequests { UserId = Core.CurrentUser.Id, BookId = book.Id, CreatedAt = DateTime.Now };
            Core.Context.UnfreezeRequests.Add(req);
            Core.Context.SaveChanges();
            MessageBox.Show("Апелляция на книгу отправлена.");
        }
    }
}