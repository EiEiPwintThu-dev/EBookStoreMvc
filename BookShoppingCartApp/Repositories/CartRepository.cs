using Microsoft.AspNetCore.Identity;

namespace BookShoppingCartApp.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public CartRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _dbContext = context;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            var transaction = _dbContext.Database.BeginTransaction();
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
                    _dbContext.ShoppingCarts.Add(cart);
                }
                _dbContext.SaveChanges();

                var cartItem = await _dbContext.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if(cartItem is not null) cartItem.Quantity += qty;
                else
                {
                    var book = _dbContext.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _dbContext.CartDetails.Add(cartItem);
                }
                _dbContext.SaveChanges();
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
          
                var cartItem = await _dbContext.CartDetails
                    .FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                
                if (cartItem is null) throw new Exception("Cart Item not found.");
                else if (cartItem.Quantity == 1) _dbContext.CartDetails.Remove(cartItem);
                else cartItem.Quantity--;
  
                _dbContext.SaveChanges();
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
                
                var cart = await _dbContext.ShoppingCarts
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
            var result = await _dbContext.ShoppingCarts.FirstOrDefaultAsync(cart => cart.UserId == userId);
            return result;
        }

        public async Task<int> GetCartItemCount(string userId ="")
        {
            userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return 0;
            var result = await (from cart in _dbContext.ShoppingCarts
                         join cartDetail in _dbContext.CartDetails
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

        public async Task<bool> DoCheckOut()
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (userId is null) throw new Exception("User is not logged-in;");

                var cart = await GetCart(userId);
                if (cart is null) throw new Exception("Invalid Cart;");

                var cartDetail = await _dbContext.CartDetails.Where(a => a.ShoppingCartId == cart.Id).ToListAsync();
                if (cartDetail.Count == 0) throw new Exception("Cart is empty.");

                var order = new Order
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    OrderStatusId = 1, //Pending
                };
                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();

                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _dbContext.OrderDetails.Add(orderDetail);
                }
                _dbContext.SaveChanges();

                _dbContext.CartDetails.RemoveRange(cartDetail);
                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
