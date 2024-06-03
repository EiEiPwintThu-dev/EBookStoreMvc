namespace BookShoppingCartApp.Models.Dtos
{
    public class BookViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public string SearchItem { get; set; } = "";
        public int GenreId { get; set; } = 0;
    }
}
