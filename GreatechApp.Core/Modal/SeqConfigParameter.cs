using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace GreatechApp.Core.Modal
{
    public class SeqConfigParameter : BindableBase
    {
        public int Id { get; set; }

        private string m_Description;
        public string Description
        {
            get { return m_Description; }
            set { SetProperty(ref m_Description, value); }
        }

        private string m_ToolTip;
        public string ToolTip
        {
            get { return m_ToolTip; }
            set { SetProperty(ref m_ToolTip, value); }
        }

        private float m_Value;
        public float Value
        {
            get { return m_Value; }
            set { SetProperty(ref m_Value, value); }
        }

        private float m_Min;
        public float Min
        {
            get { return m_Min; }
            set { SetProperty(ref m_Min, value); }
        }

        private float m_Max;
        public float Max
        {
            get { return m_Max; }
            set { SetProperty(ref m_Max, value); }
        }

        private ObservableCollection<OptionMember> m_OptionMemberCollection;
        public ObservableCollection<OptionMember> OptionMemberCollection
        {
            get { return m_OptionMemberCollection; }
            set { SetProperty(ref m_OptionMemberCollection, value); }
        }

        private OptionMember m_SelectedOption;
        public OptionMember SelectedOption
        {
            get { return m_SelectedOption; }
            set { SetProperty(ref m_SelectedOption, value); }
        }

        #region Prev Data
        public float PrevValue { get; set; }
        public string PrevDescription { get; set; }
        public float PrevMin { get; set; }
        public float PrevMax { get; set; }
        public OptionMember PrevSelectedOption { get; set; }
        #endregion
    }

    public class OptionMember : BindableBase
    {
        private int m_ChoiceId;
        public int ChoiceId
        {
            get { return m_ChoiceId; }
            set { SetProperty(ref m_ChoiceId, value); }
        }

        private string m_ChoiceDesc;
        public string ChoiceDesc
        {
            get { return m_ChoiceDesc; }
            set { SetProperty(ref m_ChoiceDesc, value); }
        }
    }

}
