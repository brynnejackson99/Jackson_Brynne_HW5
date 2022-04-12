using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Jackson_Brynne_HW5.Models
{
    public class OrderDetail
    {
        public Int32 OrderDetailID { get; set; }

        [Required(ErrorMessage = "You must specify a quantity")]
        [Display(Name = "Quantity: ")]
        [Range(1, 100000000000, ErrorMessage = "Order quantity must be at least 1")]
        public Int32 Quantity { get; set; }

        //TODO: Figure out how to do product price and extended price
        [Display(Name = "Product Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ProductPrice { get; set; }
        //OrderDetail.Price = Product.Price


        [Display(Name = "Extended Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ExtendedPrice { get; set; }


        public Order Order { get; set; }
        public Product Product { get; set; }

    }
}