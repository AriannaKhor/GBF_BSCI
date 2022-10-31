namespace GreatechApp.Core.Utilities
{
    using System;
    using GreatechApp.Core.Events;
    using GreatechApp.Core.Variable;
    using Prism.Events;

    public class LotOperation
    {
        #region Variable


        IEventAggregator _eventAggregator;


        #endregion

        #region Constructor

        public LotOperation(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            //MySqlOperation.GetActiveLotNumber();
        }

        #endregion

        #region Method

        public void StartLot(string LotID, string OperatorID, string Recipe)
        {
            Global.StartDateTime = DateTime.Now;
            Global.DownTime = new TimeSpan();
            Global.IdleTime = new TimeSpan();
            //MySqlOperation.InsertLotNumber(LotID, OperatorID, Recipe, Global.StartDateTime);
            Global.LotNumber = LotID;
            Global.Recipe = Recipe;
            Global.OperatorID = OperatorID;

            _eventAggregator.GetEvent<UpdateUI>().Publish();
        }

        public void Endlot()
        {
           // MySqlOperation.UpdateEndLot();
            Global.LotNumber = "";
            Global.Recipe = "";
            Global.OperatorID = "";
            Global.TotalPass = 0;
            Global.TotalIN = 0;
            // Serialization.OEESerialize(new OEEItems());
            _eventAggregator.GetEvent<UpdateUI>().Publish();
        }

        #endregion

    }
}
