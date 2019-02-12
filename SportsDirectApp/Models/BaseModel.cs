using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportsDirectApp.Models
{
    public abstract class BaseModel
    {
        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime DateModified { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public string ModifiedBy { get; set; }

        public void SetCreateProperties(string username)
        {
            var now = DateTime.Now;
            DateCreated = now;
            DateModified = now;
            CreatedBy = username;
            ModifiedBy = username;
        }

        public void SetEditProperties(string username)
        {
            DateModified = DateTime.Now;
            ModifiedBy = username;
        }
    }
}
