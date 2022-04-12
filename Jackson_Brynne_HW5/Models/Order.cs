using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Jackson_Brynne_HW5.Models
{
    public class Order
    {
        private const Decimal TAX_RATE = 0.0825m;

        //add scalar properties here
        public Int32 OrderID { get; set; }

        //TODO: figure out how to get num to range 7000 up
        [Display(Name = "Order Number:")]
        public Int32 OrderNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Order Notes: ")]
        public String OrderNotes { get; set; }



        [Display(Name = "Order Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal OrderSubtotal
        {
            get { return OrderDetails.Sum(rd => rd.ExtendedPrice); }
        }

        [Display(Name = "Sales tax (8.25%)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal SalesTax
        {
            get { return OrderSubtotal * TAX_RATE; }
        }

        [Display(Name = "Order Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal OrderTotal
        {
            get { return OrderSubtotal + SalesTax; }
        }




        public List<OrderDetail> OrderDetails { get; set; }
        public AppUser User { get; set; }




        //if you get a null reference error we don't have enough info to know what causes it
        //This might be causing the null reference error because we need a constructor for a list
        //TODO: figure out what needs to actually be in this list based on the relationships
        public Order()
        {
            if (OrderDetails == null)
            {
                OrderDetails = new List<OrderDetail>();
            }

        }
    }
}
