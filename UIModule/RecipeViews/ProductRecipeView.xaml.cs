﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UIModule.RecipeViews
{
    /// <summary>
    /// Interaction logic for ProductRecipe.xaml
    /// </summary>
    public partial class ProductRecipeView : UserControl
    {
        public ProductRecipeView()
        {
#if DEBUG || SIMULATION
            //only perform the following fix if we are in the designer
            // - the default ctor is not executed when editing the usercontrol,
            //   but is executed when usercontrol has been added to a window/page
            // NB. The Visual Studio designer might return null for Application.Current
            //     http://msdn.microsoft.com/en-us/library/bb546934.aspx
            if (DesignerProperties.GetIsInDesignMode(this) && Application.Current != null)
            {
                Uri resourceLocater =
                    new System.Uri("/GreatechApp.Core;component/Cultures/CultureRD.xaml",
                        UriKind.Relative);
                ResourceDictionary dictionary =
                    (ResourceDictionary)Application.LoadComponent(resourceLocater);
                //add the resourcedictionary containing our Resources ODP to
                //App.Current (which is the Designer / Blend)
                if (!Application.Current.Resources.MergedDictionaries.Contains(dictionary))
                    Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
#endif
            InitializeComponent();
        }
    }
}
