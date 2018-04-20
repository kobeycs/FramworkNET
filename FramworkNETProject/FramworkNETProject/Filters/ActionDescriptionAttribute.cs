using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=false)]
    public class ActionDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public ActionDescriptionAttribute(string Description)
        {
            this.Description = Description;
        }
    }
}