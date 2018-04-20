using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public interface ITreeData<T>
    {
        List<T> Children { get; set; }
        T Parent { get; set; }
    }
}
