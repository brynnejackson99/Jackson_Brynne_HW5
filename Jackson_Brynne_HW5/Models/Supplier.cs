using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Jackson_Brynne_HW5.Models
{
    public class Supplier
    {
        //add scalar properties
        public Int32 SupplierID { get; set; }

        [Required(ErrorMessage = "Supplier name is required!")]
        [Display(Name = "Supplier Name:")]
        public String SupplierName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [Display(Name = "Email:")]
        public String Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required!")]
        [Display(Name = "Phone Number:")]
        public String PhoneNumber { get; set; }

        public List<Product> Products { get; set; }

        //if you get a null reference error we don't have enough info to know what causes it
        //This might be causing the null reference error because we need a constructor for a list
        //TODO: figure out what needs to actually be in this list based on the relationships
        public Supplier()
        {
            if (Products == null)
            {
                Products = new List<Product>();
            }
        }
    }
}
