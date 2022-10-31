using Prism.Commands;
using GreatechApp.Core.Events;

namespace UIModule.MainPanel
{
    public class EStopViewModel : BaseUIViewModel
    {
        #region Variable
        public DelegateCommand EStopCommand { get; set; }
        #endregion

        #region Constructor
        public EStopViewModel()
        {
            EStopCommand = new DelegateCommand(OnEStopOperation);
        }
        #endregion

        #region Event
        private void OnEStopOperation()
        {
            m_EventAggregator.GetEvent<EStopOperation>().Publish();
        }
        #endregion
    }
}
