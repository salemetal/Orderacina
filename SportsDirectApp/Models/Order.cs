using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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

        private decimal? _total;
        public decimal Total
        {
            get
            {
                if (_total == null)
                {
                    if(OrderItems == null || !OrderItems.Any())
                    {
                        _total = 0;
                    }
                    else
                    {
                        _total = OrderItems.Sum(i => i.Amount * i.Price);
                    }
                }

                return (decimal)_total;
            }
        }

        #region Public Methods
        public string GenerateCompleteCalculationHtml(string HNBApiTecajType)
        {
            string html = string.Empty;

            var tecaj = Common.Utility.GetTecaj(Currency, HNBApiTecajType);

            var calcCollection = new UserCalculationCollection(tecaj, this);
            calcCollection.Init();

            html = calcCollection.CreateHTMLContent();

            return html;
        }

        public string GenerateBillCalculationHtml(string HNBApiTecajType)
        {
            var tecaj = Common.Utility.GetTecaj(Currency, HNBApiTecajType);

            var calcCollection = new UserCalculationCollection(tecaj, this);
            calcCollection.Init();

            return calcCollection.CreateBillTable();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
