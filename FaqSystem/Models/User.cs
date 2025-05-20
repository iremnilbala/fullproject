using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FaqSystem.Models
{
    public class User : IdentityUser
    {

        [DisplayName("Username")]
        public override string UserName { get; set; }

        [DataType("Password")]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        [DataType("Password")]
        [DisplayName("Password")]
        public string PasswordConfirmation { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Surname")]
        public string Surname { get; set; }

        [DisplayName("PhoneNumber")]
        public override string PhoneNumber { get; set; }

        [DisplayName("isAdmin")]
        public bool isAdmin { get; set; }

        [NotMapped]
        public string FullName => Name + " " + Surname;

        public ICollection<Faq> Faqs { get; set; }
    }
}