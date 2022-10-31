using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace GreatechApp.Services.Utilities
{
    public class SQLOperation : ISQLOperation
    {
        static IEventAggregator m_EventAggregator;
        static CultureResources m_CultureResources;

        public SQLOperation(IEventAggregator eventAggregator,CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_CultureResources = cultureResources;
        }

        public ServiceController serviceCtrl { get; set; }

        public bool EvalSQLService(object sqlKey)
        {
            serviceCtrl.Refresh();

            if (serviceCtrl.Status != ServiceControllerStatus.Running)
            {
                if (Monitor.TryEnter(sqlKey))
                {
                    try
                    {
                        if (serviceCtrl.Status != ServiceControllerStatus.Stopped && serviceCtrl.Status != ServiceControllerStatus.StopPending)
                        {
                            serviceCtrl.Stop();
                            serviceCtrl.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                        else if (serviceCtrl.Status == ServiceControllerStatus.StopPending)
                        {
                            serviceCtrl.WaitForStatus(ServiceControllerStatus.Stopped);
                        }

                        serviceCtrl.Start();
                        serviceCtrl.WaitForStatus(ServiceControllerStatus.Running);
                        return true;
                    }
                    finally
                    {
                        Monitor.Exit(sqlKey);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        #region User Management
        public bool AddUser(string userID, string userName, int userLevel, string password)
        {
            using (var db = new AppDBContext())
            {
                try
                {
                    var user = new TblUser
                    {
                        User_ID = userID,
                        UserName = userName,
                        User_Level = userLevel,
                        Password = password
                    };
                    db.TblUser.Add(user);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("AddUser")} {m_CultureResources.GetStringValue("Fail")} - {ex.Message}]" });
                    return false;
                }
            }
        }

        public bool IsValidEntry(ObservableCollection<TblUser> userCollection, string userID)
        {
            try
            {
                using (var db = new AppDBContext())
                {
                    if (!userCollection.Any(x => x.User_ID == userID))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("IsValidEntryCheck")} {m_CultureResources.GetStringValue("Fail")} - {ex.Message}]" });
                return false;
            }

        }

        public bool EditUser(string userID, string userName, int userLevel, string password)
        {
            try
            {
                using (var db = new AppDBContext())
                {
                    TblUser user = db.TblUser.Single(x => x.User_ID == userID);
                    user.UserName = userName;
                    user.User_Level = userLevel;
                    user.Password = password;
                    db.TblUser.Update(user);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("Update")} {m_CultureResources.GetStringValue("User")} {m_CultureResources.GetStringValue("Fail")}- {ex.Message}]" });
                return false;
            }
        }

        public bool DeleteUser(string userID)
        {
            try
            {
                using (var db = new AppDBContext())
                {
                    TblUser user = db.TblUser.Single(x => x.User_ID == userID);
                    db.TblUser.Remove(user);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("Delete")} {m_CultureResources.GetStringValue("User")} {m_CultureResources.GetStringValue("Fail")}- {ex.Message}]" });
                return false;
            }
        }

        public bool UpdateAcessControl(TblAccessControl selectedAccessControl)
        {
            try
            {
                using (var db = new AppDBContext())
                {
                    TblAccessControl accCtrl = db.TblAccessControl.Single(x => x.User_Level == selectedAccessControl.User_Level);
                    accCtrl.User_Desc = selectedAccessControl.User_Desc;
                    accCtrl.Setting = selectedAccessControl.Setting;
                    accCtrl.IO = selectedAccessControl.IO;
                    accCtrl.Communication = selectedAccessControl.Communication;
                    accCtrl.Motion = selectedAccessControl.Motion;
                    accCtrl.User_Management = selectedAccessControl.User_Management;
                    db.TblAccessControl.Update(accCtrl);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("UpdateAccessControl")} {m_CultureResources.GetStringValue("Fail")}- {ex.Message}]" });
                return false;
            }
        }
        #endregion

        #region Lot Entry
        public bool InsertNewLot(string lotID, string operatorID, string LotTotalBatch,string recipeName, DateTime startDateTime)
        {
            using (var db = new AppDBContext())
            {
                try
                {
                    var lotdata = new TblLot
                    {
                        Lot_Number = lotID,
                        Operator_ID = operatorID,
                        Recipe = recipeName,
                        Start_Time = startDateTime,
                        End_Time = startDateTime,
                        Total_Input = 0,
                        Total_Output = 0
                    };
                    db.TblLot.Add(lotdata);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("AddLotInfo")} {m_CultureResources.GetStringValue("Fail")} - {ex.Message}]" });
                    return false;
                }
            }
        }

        public void UpdateLotData(string LotID, DateTime endLotDateTime, int totalInput, int totalOutput)
        {
            using (var db = new AppDBContext())
            {
                try
                {
                    TblLot lot = db.TblLot.Single(x => x.Lot_Number == LotID);
                    lot.End_Time = endLotDateTime;
                    lot.Total_Input = totalInput;
                    lot.Total_Output = totalOutput;
                    db.TblLot.Update(lot);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("Database")}, {m_CultureResources.GetStringValue("UpdateLotInfo")} {m_CultureResources.GetStringValue("Fail")}- {ex.Message}]" });
                }
            }
        }
        #endregion

        #region Error Log
        public void AddErrorToDB(DateTime endTime, DateTime startTime, string machineName, string station, string lotNumber, int errorID, string errorType, string errorDesc)
        {
            using (var db = new AppDBContext())
            {
                var errordata = new TblError();
                errordata.Stop_Time = endTime;
                errordata.Start_Time = startTime;
                errordata.Machine_ID = machineName;
                errordata.Station = station;
                errordata.Lot_Number = lotNumber;
                errordata.Error_ID = errorID;
                errordata.Error_Type = errorType;
                errordata.Error_Desc = errorDesc;
                db.TblError.Add(errordata);
                db.SaveChanges();
            }
        }
        #endregion

        #region Recipe
        public void AddRecipeToDB(string productName, int productLoopCount, int productIntervalDistance, double productIntervalDelay)
        {
            using (var db = new AppDBContext())
            {
                var recipe = new TblRecipe();
                recipe.Product_Name = productName;
                recipe.Loop_Count = productLoopCount;
                recipe.Interval_Distance = productIntervalDistance;
                recipe.Interval_Delay = productIntervalDelay;
                db.TblRecipe.Add(recipe);
                db.SaveChanges();
            }
        }

        public void UpdateRecipeToDB(int id, string productName, int productLoopCount, int productIntervalDistance, double productIntervalDelay)
        {
            using (var db = new AppDBContext())
            {
                TblRecipe recipe = db.TblRecipe.Single(x => x.Id == id);
                recipe.Product_Name = productName;
                recipe.Loop_Count = productLoopCount;
                recipe.Interval_Distance = productIntervalDistance;
                recipe.Interval_Delay = productIntervalDelay;
                db.TblRecipe.Update(recipe);
                db.SaveChanges();
            }
        }

        public void DeleteRecipeFromDB(int id)
        {
            using (var db = new AppDBContext())
            {
                TblRecipe recipe = db.TblRecipe.Single(x => x.Id == id);
                db.TblRecipe.Remove(recipe);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
