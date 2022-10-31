using System;
using System.Windows;
using System.Windows.Markup;

namespace Resources
{
    public abstract class StyleRefExtension : MarkupExtension
    {
        /// <summary>
        /// Property for specific resource dictionary
        /// </summary>
        protected static ResourceDictionary RD;

        /// <summary>
        /// Resource key wich we want to extract
        /// </summary>
        public String ResourceKey { get; set; }

        /// <summary>
        /// Overriding base function wich will return key from RD
        /// </summary>
        /// <param name="serviceProvider">Not used</param>
        /// <returns>Object from RD</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(RD == null) throw new Exception(
                @"You should define RD before usage. 
                Please make it in staticc cconstructor of extending class!");
            return RD[ResourceKey];
        }
    }
}
