using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class ProductQtyViewModel : BaseUIViewModel
    {
        #region Variable 
        public ProductQtyConfig ProductQtyConfig;
        #endregion
        #region Properties 
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private ObservableCollection<ProductQuantityParameter> m_ProductQuantityLimit;
        public ObservableCollection<ProductQuantityParameter> ProductQuantityLimit
        {
            get { return m_ProductQuantityLimit; }
            set { SetProperty(ref m_ProductQuantityLimit, value); }
        }
        #endregion

        #region Constructor
        public ProductQtyViewModel()
        {
            TabPageHeader = GetStringTableValue("ProductQuantity");

            GetProductQuantityConfig();  
        }
        #endregion

        #region Method
        public void GetProductQuantityConfig()
        {
            ProductQtyConfig prdqtyconfig = ProductQtyConfig.Open(m_SystemConfig.PrdQtyLimitCfg[0].Reference);
            ProductQuantityLimit = new ObservableCollection<ProductQuantityParameter>();
            int countLimit = prdqtyconfig.Setting.Count;
            for (int limit = 0; limit < countLimit; limit++)
            {
                ProductQuantityLimit.Add(new ProductQuantityParameter()
                {
                    Id = prdqtyconfig.Setting[limit].Id,
                    Description = prdqtyconfig.Setting[limit].Description,
                    Min = prdqtyconfig.Setting[limit].MinQuantity,
                    Max = prdqtyconfig.Setting[limit].MaxQuantity,

                    PrevId = prdqtyconfig.Setting[limit].Id,
                    PrevDescription = prdqtyconfig.Setting[limit].Description,
                    PrevMin = prdqtyconfig.Setting[limit].MinQuantity,
                    PrevMax = prdqtyconfig.Setting[limit].MaxQuantity,

                });
            }
        }

        public void DataGridCellChangedMethod(object sender, DataGridRowEditEndingEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            int rowIndex = e.Row.GetIndex();

            ObservableCollection<ProductQuantityParameter> configParam = new ObservableCollection<ProductQuantityParameter>();
            ProductQtyConfig prdqtyconfig = ProductQtyConfig.Open(m_SystemConfig.PrdQtyLimitCfg[0].Reference);

            switch (datagrid.Name)
            {
                case "ProductQuantity":
                    configParam = ProductQuantityLimit;
                    break;
            }

            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, $"{GetDialogTableValue("AskSaveChange")} {datagrid.Name} ?");
            if (dialogResult == ButtonResult.Yes)
            {
                if(configParam[rowIndex].Description != configParam[rowIndex].PrevDescription)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} : {configParam[rowIndex].Description}, {GetStringTableValue("PrevValue")} : {configParam[rowIndex].PrevDescription} ; {GetStringTableValue("NewValue")} : {configParam[rowIndex].Description}" });
                }
                else if(configParam[rowIndex].Min != configParam[rowIndex].PrevMin)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} : {configParam[rowIndex].Description}, {GetStringTableValue("PrevValue")} : {configParam[rowIndex].PrevMin} ; {GetStringTableValue("NewValue")} : {configParam[rowIndex].Min}" });
                }
                else if (configParam[rowIndex].Max != configParam[rowIndex].PrevMax)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} : {configParam[rowIndex].Description}, {GetStringTableValue("PrevValue")} : {configParam[rowIndex].PrevMax} ; {GetStringTableValue("NewValue")} : {configParam[rowIndex].Max}" });
                }

                configParam[rowIndex].PrevId = configParam[rowIndex].Id;
                configParam[rowIndex].PrevDescription = configParam[rowIndex].Description;
                configParam[rowIndex].PrevMin = configParam[rowIndex].Min;
                configParam[rowIndex].PrevMax = configParam[rowIndex].Max;

                int rowIndexId = configParam[rowIndex].Id;

                switch (datagrid.Name)
                {
                    case "ProductQuantity":
                        prdqtyconfig.Setting[rowIndexId].Id = configParam[rowIndex].Id;
                        prdqtyconfig.Setting[rowIndexId].Description = configParam[rowIndex].Description;
                        prdqtyconfig.Setting[rowIndexId].MinQuantity = configParam[rowIndex].Min;
                        prdqtyconfig.Setting[rowIndexId].MaxQuantity = configParam[rowIndex].Max;
                        break;
                }

                ProductQtyConfig.Save(prdqtyconfig);
            }
            else if (dialogResult == ButtonResult.Cancel)
            {
                configParam[rowIndex].Id = configParam[rowIndex].PrevId;
                configParam[rowIndex].Description = configParam[rowIndex].PrevDescription;
                configParam[rowIndex].Min = configParam[rowIndex].PrevMin;
                configParam[rowIndex].Max = configParam[rowIndex].PrevMax;
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("ProductQuantity");
        }
        #endregion


    }
}
