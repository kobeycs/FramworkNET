using System;

namespace Models.Attributes
{
    public class MLRequiredAttribute : Attribute
    {
        public bool AllLanguageRequired { get; set; }

        public MLRequiredAttribute()
        {
            AllLanguageRequired = true;
        }
    }
}
