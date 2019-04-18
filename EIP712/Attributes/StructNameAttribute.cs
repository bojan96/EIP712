using System;

namespace EIP712.Attributes
{
    public class StructNameAttribute : Attribute
    {
        public string Name { get; }

        public StructNameAttribute(string name)
            => Name = name ?? throw new ArgumentNullException(nameof(name));
        
    }
}
