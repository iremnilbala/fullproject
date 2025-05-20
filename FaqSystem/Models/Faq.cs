using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FaqSystem.Models
{
    public class Faq
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Question")]
        public string Question { get; set; }

        [DisplayName("Answer")]
        public string Answer { get; set; }

        [DisplayName("Is this faq deleted?")]
        public bool isDeleted { get; set; }

        [DisplayName("Create Date")]
        public DateTime CreateDate { get; set; }

        [DisplayName("Update Date")]
        public DateTime? UpdateDate { get; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}