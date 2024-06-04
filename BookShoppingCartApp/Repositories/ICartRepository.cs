namespace BookShoppingCartApp.Repositories
{
    public interface ICartRepository
    {
        public Task<int> AddItem(int bookId, int qty);
        public Task<int> RemoveItem(int bookId);
        public Task<ShoppingCart> GetUserCart();
        public Task<ShoppingCart> GetCart(string userId);
        public Task<int> GetCartItemCount(string userId = "");
        public Task<bool> DoCheckOut();
    }
}
