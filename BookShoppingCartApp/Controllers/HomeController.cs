using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookShoppingCartApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;
        }

        public async Task<IActionResult> Index(string sTerm = "", int genreId = 0)
       {
            var genres = await _homeRepository.GetAllGenres();
            var books = await _homeRepository.GetBooks(sTerm, genreId);

            var bookViewModel = new BookViewModel
            {
                Genres = genres,
                Books = books,
                SearchItem = sTerm,
                GenreId = genreId
            };
            return View(bookViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
