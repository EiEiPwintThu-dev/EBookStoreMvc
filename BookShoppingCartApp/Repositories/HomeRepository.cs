using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartApp.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        public readonly ApplicationDbContext _context;
        public HomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string searchItem = "", int genreId = 0)
        {
            searchItem = searchItem.ToLower();
            var books = await (from book in _context.Books
                               join genre in _context.Genres
                               on book.GenreId equals genre.Id
                               where string.IsNullOrWhiteSpace(searchItem) || (book != null && book.BookName.ToLower().StartsWith(searchItem))
                               select new Book
                               {
                                   Id = book.Id,
                                   Image = book.Image,
                                   AuthorName = book.AuthorName,
                                   BookName = book.BookName,
                                   GenreId = book.GenreId,
                                   Price = book.Price,
                                   GenreName = genre.GenreName
                               }).ToListAsync();

            if (genreId > 0) books = books.Where(b => b.GenreId == genreId).ToList();
            return books;
        }
    }
}
