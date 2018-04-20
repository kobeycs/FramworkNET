using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public abstract class MLContent<TParent> : MLContent
    {
        public TParent MLParent { get; set; }
    }

    public abstract class MLContent : BasePoco
    {
        public string LanguageCode { get; set; }
        public long MLParentID { get; set; }
    }
}