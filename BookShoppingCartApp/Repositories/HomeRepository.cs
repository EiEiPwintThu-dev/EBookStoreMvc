using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartApp.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        public readonly ApplicationDbContext _dbContext;
        public HomeRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Genre>> GetAllGenres()
        {
            return await _dbContext.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string searchItem = "", int genreId = 0)
        {
            searchItem = searchItem.ToLower();
            var books = await (from book in _dbContext.Books
                               join genre in _dbContext.Genres
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
