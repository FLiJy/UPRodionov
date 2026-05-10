using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class AddEditBookPage : Page
    {
        private Books _currentBook;

        public AddEditBookPage(Books book)
        {
            InitializeComponent();
            _currentBook = book;

            if (_currentBook != null)
            {
                TbHeader.Text = "Редактирование";
                TbTitle.Text = _currentBook.Title;
                TbDesc.Text = _currentBook.Description;
                // TbContent.Text = ... загрузка текста
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbTitle.Text)) { MessageBox.Show("Укажите название!"); return; }

            if (_currentBook == null)
            {
                _currentBook = new Books { AuthorId = Core.CurrentUser.Id, IsFrozen = false };
                Core.Context.Books.Add(_currentBook);
            }

            _currentBook.Title = TbTitle.Text;
            _currentBook.Description = TbDesc.Text;

            Core.Context.SaveChanges();
            MessageBox.Show("Данные сохранены!");
            NavigationService.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}