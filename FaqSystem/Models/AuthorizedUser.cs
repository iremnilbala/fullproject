using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FaqSystem.Models
    {
        public class AuthorizedUser
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Surname { get; set; }
        }
    }
