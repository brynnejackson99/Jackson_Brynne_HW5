using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Jackson_Brynne_HW5.Models
{
    public enum ProductType { hat, pants, sweatshirt, tank, Tshirt, other }
    public class Product
    {
        //Primary key
        public Int32 ProductID { get; set; }

        //Navigational properties
        [Required(ErrorMessage = "Product name is required!")]
        [Display(Name = "Product Name:")]
        public String Name { get; set; }

        [Display(Name = "Desciption:")]
        public String Description { get; set; }


        [Required(ErrorMessage = "Price is required!")]
        [Display(Name = "Price:")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public Decimal Price { get; set; }

        
        [Display(Name = "Product Type:")]
        public ProductType Type { get; set; }


        public List<Supplier> Suppliers { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        //if you get a null reference error we don't have enough info to know what causes it
        //This might be causing the null reference error because we need a constructor for a list
        //TODO: figure out what needs to actually be in this list based on the relationships
        public Product()
        {
            if (Suppliers == null)
            {
                Suppliers = new List<Supplier>();
            }
            if (OrderDetails == null)
            {
                OrderDetails = new List<OrderDetail>();
            }
        }
    }
}
