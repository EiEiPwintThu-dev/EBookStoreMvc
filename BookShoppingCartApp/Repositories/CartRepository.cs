using Microsoft.AspNetCore.Identity;

namespace BookShoppingCartApp.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public CartRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId)) throw new Exception("Invalid User Id");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                    };
                    _context.ShoppingCarts.Add(cart);
                }
                _context.SaveChanges();

                var cartItem = await _context.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if(cartItem is not null) cartItem.Quantity += qty;
                else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty
                    };
                    _context.CartDetails.Add(cartItem);
                }
                _context.SaveChanges();
                transaction.Commit();

                var totalCount = await GetCartItemCount(userId);
                return totalCount;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> RemoveItem(int bookId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId)) throw new Exception("Invalid user id");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Cart not found.");
                }
          
                var cartItem = await _context.CartDetails
                    .FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                
                if (cartItem is null) throw new Exception("Cart Item not found.");
                else if (cartItem.Quantity == 1) _context.CartDetails.Remove(cartItem);
                else cartItem.Quantity--;
  
                _context.SaveChanges();
                var totalCount = await GetCartItemCount(userId);
                return totalCount;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) throw new Exception("Invalid User Id.");
                
                var cart = await _context.ShoppingCarts
                    .Include(cart => cart.CartDetails)
                    .ThenInclude(cartDetail => cartDetail.Book)
                    .ThenInclude(book => book.Genre)
                    .Where(cart => cart.UserId == userId)
                    .FirstOrDefaultAsync();
                return cart;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var result = await _context.ShoppingCarts.FirstOrDefaultAsync(cart => cart.UserId == userId);
            return result;
        }

        public async Task<int> GetCartItemCount(string userId ="")
        {
            userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return 0;
            var result = await (from cart in _context.ShoppingCarts
                         join cartDetail in _context.CartDetails
                         on cart.Id equals cartDetail.ShoppingCartId
                         where cart.UserId == userId
                         select new
                         {
                             cartDetail.Id
                         }).ToListAsync();
            return result.Count;
        }

        private string GetUserId()
        {
            var principal = _contextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
