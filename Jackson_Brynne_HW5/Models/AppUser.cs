using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;


namespace Jackson_Brynne_HW5.Models
{
    public class AppUser:IdentityUser
    {
        
        //First name is provided as an example
        public String FirstName { get; set; }
        public String LastName { get; set; }
    }
}
