using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Models
{
    public interface IMLData<T> : IMLData where T:MLContent
    {
        List<T> MLContents { get; set; }
    }

    public interface IMLData
    {
    }
}
