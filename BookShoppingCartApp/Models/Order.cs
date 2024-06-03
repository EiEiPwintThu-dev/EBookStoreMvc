using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCartApp.Models
{
    [Table("Order")]
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public bool IsDeleted { get; set; } = false;

        public List<OrderDetail> OrderDetail { get; set; }
    }
}
