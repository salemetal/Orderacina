using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportsDirectApp.Models
{
    public class Order : BaseModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsOpen { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        [Required(ErrorMessage = "Pick a Shop!")]
        public int ShopId { get; set; }
        public Shop Shop { get; set; }

        [DisplayFormat(DataFormatString = "{0,00}", ApplyFormatInEditMode = true)]
        public decimal? Shipping { get; set; }

        [Required]
        public string Currency { get; set; }

    }
}
