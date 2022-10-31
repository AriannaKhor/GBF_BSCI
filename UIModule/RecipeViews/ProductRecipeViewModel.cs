using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Resources;
using GreatechApp.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;
using UIModule.MainPanel;

namespace UIModule.RecipeViews
{
    public class ProductRecipeViewModel : BaseUIViewModel
    {
        #region Variable
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private bool m_IsAllowUpdate;
        public bool IsAllowUpdate
        {
            get { return m_IsAllowUpdate; }
            set { SetProperty(ref m_IsAllowUpdate, value); }
        }

        private bool m_IsAllowDelete;
        public bool IsAllowDelete
        {
            get { return m_IsAllowDelete; }
            set { SetProperty(ref m_IsAllowDelete, value); }
        }

        #region Database Recipe
        private AppDBContext m_DBContext;

        private ObservableCollection<TblRecipe> m_Recipes;
        public ObservableCollection<TblRecipe> Recipes
        {
            get { return m_Recipes; }
            set
            {
                SetProperty(ref m_Recipes, value);
                if(Recipes.Count > 0)
                {
                    if (SelectedRecipe == null || Recipes.FirstOrDefault(x => x.Id == SelectedRecipe.Id) == null)
                    {
                        SelectedRecipe = Recipes[0];
                    }
                }
            }
        }

        private TblRecipe m_SelectedRecipe;
        public TblRecipe SelectedRecipe
        {
            get { return m_SelectedRecipe; }
            set
            {
                SetProperty(ref m_SelectedRecipe, value);

                IsAllowUpdate = SelectedRecipe != null;
                IsAllowDelete = SelectedRecipe != null;

                if (SelectedRecipe == null)
                {
                    return;
                }
                SelectedRecipeID = SelectedRecipe.Id;
                SelectedProductName = SelectedRecipe.Product_Name;
                SelectedProductLoopCount = SelectedRecipe.Loop_Count;
                SelectedProductIntervalDistance = SelectedRecipe.Interval_Distance;
                SelectedProductIntervalDelay = SelectedRecipe.Interval_Delay;
            }
        }

        private int m_SelectedProductIntervalDistance;
        public int SelectedProductIntervalDistance
        {
            get { return m_SelectedProductIntervalDistance; }
            set { SetProperty(ref m_SelectedProductIntervalDistance, value); }
        }

        private int m_SelectedProductLoopCount;
        public int SelectedProductLoopCount
        {
            get { return m_SelectedProductLoopCount; }
            set { SetProperty(ref m_SelectedProductLoopCount, value); }
        }

        private double m_SelectedProductIntervalDelay;
        public double SelectedProductIntervalDelay
        {
            get { return m_SelectedProductIntervalDelay; }
            set { SetProperty(ref m_SelectedProductIntervalDelay, value); }
        }

        private string m_SelectedProductName;
        public string SelectedProductName
        {
            get { return m_SelectedProductName; }
            set { SetProperty(ref m_SelectedProductName, value); }
        }

        private int m_SelectedRecipeID;
        public int SelectedRecipeID
        {
            get { return m_SelectedRecipeID; }
            set { SetProperty(ref m_SelectedRecipeID, value); }
        }

        private string m_Status;
        public string Status
        {
            get { return m_Status; }
            set { SetProperty(ref m_Status, value); }
        }
        #endregion

        public DelegateCommand<string> RecipeCommand { get; private set; }
        #endregion

        public ProductRecipeViewModel()
        {
            RecipeCommand = new DelegateCommand<string>(RecipeOperation);
            TabPageHeader = GetStringTableValue("ProductRecipe");

            RefreshDatabase();
        }

        #region Method
        private void RefreshDatabase()
        {
            m_DBContext = new AppDBContext();
            m_DBContext.TblRecipe.Load();
            Recipes = m_DBContext.TblRecipe.Local.ToObservableCollection();
        }

        void RecipeOperation(string Command)
        {
            if (Command == "Add")
            {
                if (IsInputOK())
                {
                    m_SQLOperation.AddRecipeToDB(SelectedProductName, SelectedProductLoopCount, SelectedProductIntervalDistance, SelectedProductIntervalDelay);
                    Status = GetDialogTableValue("RecipeAdded");
                    m_EventAggregator.GetEvent<UpdateDatabase>().Publish();
                }
            }
            else if (Command == "Update")
            {
                m_SQLOperation.UpdateRecipeToDB(SelectedRecipeID, SelectedProductName, SelectedProductLoopCount, SelectedProductIntervalDistance, SelectedProductIntervalDelay);
                Status = string.Empty;
                Status = GetDialogTableValue("RecipeUpdated");
                m_EventAggregator.GetEvent<UpdateDatabase>().Publish();
            }
            else if (Command == "Delete")
            {
                ButtonResult buttonResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmDeleteRecipe"));

                if (buttonResult == ButtonResult.Yes)
                {
                    m_SQLOperation.DeleteRecipeFromDB(SelectedRecipeID);
                    Status = string.Empty;
                    Status = GetDialogTableValue("RecipeDeleted");
                    m_EventAggregator.GetEvent<UpdateDatabase>().Publish();
                }
            }

            RefreshDatabase();
        }

        private bool IsInputOK()
        {
            if (Recipes.Any(x => x.Product_Name == SelectedProductName))
            {
                Status = string.Empty;
                Status = GetDialogTableValue("ProductExist");
                return false;
            }
            return true;
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("ProductRecipe");
        }
        #endregion
    }
}
