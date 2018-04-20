using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Models
{
    public abstract class BasePoco
    {
        [Key]
        [Display(AutoGenerateField = false)]
        public virtual long ID { get; set; }
    }
}