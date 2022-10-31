using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConfigManager
{
    public class MapObject<Product>
    {
        private Dictionary<string, Type> map = new Dictionary<string, Type>();

        public MapObject()
        {
            Type[] types = Assembly.GetAssembly(typeof(Product)).GetTypes();
            foreach (Type type in types)
            {
                if (!typeof(Product).IsAssignableFrom(type) || type == typeof(Product)) // if (type is not derived from Product)
                {
                    continue; // this type isn’t a Product type, keep searching through the the assembly

                }
                // Automatically register the Product type.
                map.Add(type.Name, type);
            }
        }

        public Product CreateObject(string productName, params object[] args)
        {
            return (Product)Activator.CreateInstance(map[productName], args);
        }
    }
}
