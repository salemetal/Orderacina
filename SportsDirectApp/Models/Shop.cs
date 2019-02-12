using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportsDirectApp.Models
{
    public class Shop
    {
        public int Id { get; set; }

        [Required]
        [Display(Name="Shop")]
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
