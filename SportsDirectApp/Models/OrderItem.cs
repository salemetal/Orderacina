using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportsDirectApp.Models
{
    public class OrderItem : BaseModel
    {
        public int Id { get; set; }

        public string Size { get; set; }

        public string Description { get; set; }

        [Required][DataType(DataType.Url)]
        public string Url { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal Total => Amount * Price;
    }
}
