using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UchebnayaPraktika;

namespace UchebnayaPraktika
{
    public partial class ListsPage : Page
    {
        public ListsPage()
        {
            InitializeComponent();
            UpdateList();
        }

        private void UpdateList()
        {
            if (Core.CurrentUser == null) return;

            string selectedStatus = (LbStatusFilter.SelectedItem as ListBoxItem)?.Tag.ToString();
            string search = TbSearch.Text.ToLower();

            // Получаем книги пользователя с конкретным статусом
            var query = Core.Context.ReadingLists
                .Where(rl => rl.UserId == Core.CurrentUser.Id && rl.Status == selectedStatus)
                .Select(rl => rl.Books)
                .Where(b => b.IsFrozen == false);

            // Поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.Title.ToLower().Contains(search) || b.Users.DisplayName.ToLower().Contains(search));
            }

            var results = query.ToList();

            // Сортировка
            if (CbSort.SelectedIndex == 1) // По оценке
                results = results.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0).ToList();
            else
                results = results.OrderBy(b => b.Title).ToList();

            LvMyBooks.ItemsSource = results;
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void FilterChanged(object sender, TextChangedEventArgs e) => UpdateList();

        private void CbChangeStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || cb.SelectedIndex == 0) return;

            int bookId = (int)cb.Tag;
            var entry = Core.Context.ReadingLists.FirstOrDefault(rl => rl.UserId == Core.CurrentUser.Id && rl.BookId == bookId);

            if (entry != null)
            {
                if (cb.SelectedIndex == 5) // Удалить
                {
                    Core.Context.ReadingLists.Remove(entry);
                }
                else
                {
                    string newStatus = "";
                    switch (cb.SelectedIndex)
                    {
                        case 1: newStatus = "Reading"; break;
                        case 2: newStatus = "Planned"; break;
                        case 3: newStatus = "Completed"; break;
                        case 4: newStatus = "Dropped"; break;
                    }
                    entry.Status = newStatus;
                }

                Core.Context.SaveChanges();
                UpdateList();
            }
        }
    }
}