namespace UIModule.StandardViews
{
    using ConfigManager;
    using DBManager.Domains;
    using GreatechApp.Core.Command;
    using GreatechApp.Core.Cultures;
    using GreatechApp.Core.Enums;
    using GreatechApp.Core.Events;
    using GreatechApp.Core.Interface;
    using GreatechApp.Core.Resources;
    using GreatechApp.Core.Variable;
    using GreatechApp.Services.UserServices;
    using Microsoft.EntityFrameworkCore;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Prism.Regions;
    using Prism.Services.Dialogs;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using UIModule.MainPanel;

    public class LotHistoryViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable
        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        #region Lot Entry
        private string m_LotID;
        public string LotID
        {
            get { return m_LotID; }
            set { SetProperty(ref m_LotID, value); }
        }

        private string m_OperatorID;
        public string OperatorID
        {
            get { return m_OperatorID; }
            set { SetProperty(ref m_OperatorID, value); }
        }

        private AppDBContext m_RecipeContext;

        private ObservableCollection<TblRecipe> m_Recipe;
        public ObservableCollection<TblRecipe> Recipe
        {
            get { return m_Recipe; }
            set 
            { 
                SetProperty(ref m_Recipe, value); 
                if(SelectedRecipe == null || Recipe.FirstOrDefault(x => x.Product_Name == SelectedRecipe.Product_Name) == null)
                {
                    if (Recipe != null && Recipe.Count > 0)
                    {
                        SelectedRecipe = Recipe[0];
                    }
                }
            }
        }

        private TblRecipe m_SelectedRecipe;
        public TblRecipe SelectedRecipe
        {
            get { return m_SelectedRecipe; }
            set { SetProperty(ref m_SelectedRecipe, value); }
        }
        #endregion
 

        private bool m_IsLotIDFocused;
        public bool IsLotIDFocused
        {
            get { return m_IsLotIDFocused; }
            set { SetProperty(ref m_IsLotIDFocused, value); }
        }

        private ObservableCollection<TblLot> m_LotHistoryCollection;
        public ObservableCollection<TblLot> LotHistoryCollection
        {
            get { return m_LotHistoryCollection; }
            set { SetProperty(ref m_LotHistoryCollection, value); }
        }

        public DelegateCommand<string> LotCommand { get; private set; }
        #endregion

        #region Constructor
        public LotHistoryViewModel()
        {
            LotHistoryCollection = new ObservableCollection<TblLot>();

            Title = GetStringTableValue("LotHistory");

            RefreshRecipes();

            LoadLotHistory();
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsLotIDFocused = true;
        }
        #endregion

        #region Events
        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("LotHistory");

        }

        #endregion

        #region Method
        public void RefreshRecipes()
        {
            m_RecipeContext = new AppDBContext();
            m_RecipeContext.TblRecipe.Load();
            Recipe = m_RecipeContext.TblRecipe.Local.ToObservableCollection();
        }

        private void LoadLotHistory()
        {
            m_RecipeContext.TblLot.FromSqlRaw("SELECT TOP 25 * FROM TblLot ORDER BY Start_Time DESC").ToList();
            LotHistoryCollection = m_RecipeContext.TblLot.Local.ToObservableCollection();
        }

        #endregion
    }
}
