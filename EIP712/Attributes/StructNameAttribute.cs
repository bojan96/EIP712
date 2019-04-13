using System;
using System.Collections.Generic;
using System.Text;

namespace EIP712.Attributes
{
    internal class StructNameAttribute : Attribute
    {
        public string Name { get; }

        public StructNameAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }
    }
}
