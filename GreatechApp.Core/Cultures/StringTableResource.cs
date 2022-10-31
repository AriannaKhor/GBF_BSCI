using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;

namespace GreatechApp.Core.Cultures
{
    public class StringTableResource : StringTable
    {
        public string GetValue(string key)
        {
            return ResourceManager.GetString(key, Global.CurrentCulture);
        }

        public string GetKey(string value)
        {
            var entry = ResourceManager.GetResourceSet(Global.CurrentCulture, true, true)
                        .OfType<DictionaryEntry>()
                        .FirstOrDefault(e => e.Value.ToString() == value);
            var key = entry.Key.ToString();
            return key;
        }
    }
}
