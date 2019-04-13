using System;
using System.Collections.Generic;
using System.Text;

namespace EIP712.Attributes
{
    internal class StructNameAttribute : Attribute
    {
        public string Name { get; }

        public StructNameAttribute(string name)
            => Name = name ?? throw new ArgumentNullException(nameof(name));
        
    }
}
