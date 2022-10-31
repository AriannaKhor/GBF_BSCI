using DBManager.Domains;
using System;
using System.Collections.ObjectModel;
using System.ServiceProcess;

namespace GreatechApp.Core.Interface
{
    public interface ISQLOperation
    {
        ServiceController serviceCtrl { get; set; }
        bool EvalSQLService(object sqlKey);

        // User Management
        bool AddUser(string userID, string userName, int userLevel, string password);
        bool DeleteUser(string userID);
        bool EditUser(string userID, string userName, int userLevel, string password);
        bool IsValidEntry(ObservableCollection<TblUser> userCollection, string userID);
        bool UpdateAcessControl(TblAccessControl selectedAccessCtrl);

        // Lot Entry
        bool InsertNewLot(string lotID, string operatorID, string totalbatchQuantity, string recipeName, DateTime startDateTime);
        void UpdateLotData(string LotID, DateTime endLotDateTime, int totalInput, int totalOutput);

        // Error Log
        void AddErrorToDB(DateTime endTime, DateTime startTime, string machineName, string station, string lotNumber, int errorID, string errorType, string errorDesc);

        // Recipe
        void AddRecipeToDB(string productName, int productLoopCount, int productIntervalDistance, double productIntervalDelay);
        void UpdateRecipeToDB(int id, string productName, int productLoopCount, int productIntervalDistance, double productIntervalDelay);
        void DeleteRecipeFromDB(int id);    
    }
}
