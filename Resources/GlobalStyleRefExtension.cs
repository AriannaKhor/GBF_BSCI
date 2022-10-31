using System;
using System.Windows;


namespace Resources
{
    public class GlobalStyleRefExtension : StyleRefExtension
    {
        static GlobalStyleRefExtension()
        {
            RD = new ResourceDictionary()
                     {
                         Source = new Uri("pack://application:,,,/Resources;component/General.xaml")
                     };
        }
    }
}
