using ConfigManager;
using GreatechApp.Core.Modal;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemManager
{
    public class GemDatabase
    {
        #region Variable
        public SecsGemConfig m_gemCfg = new SecsGemConfig();
        public SystemConfig m_SystemConfig;
        public bool enablesgconn;
        public bool EnableSGConn;
        public string deviceId;
        public string IpAddress;
        public string portID;
        public string t3;
        public string t5;
        public string t6;
        public string t7;
        public string t8;
        private string connString;

        

        #endregion

        #region Constructor
        public GemDatabase()
        {
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_gemCfg = (SecsGemConfig)ContainerLocator.Container.Resolve(typeof(SecsGemConfig));
            m_gemCfg = SecsGemConfig.Open(m_SystemConfig.GemRef[0].Reference);
            string connectionString = m_gemCfg.GPCollection[0].DatabaseDir;
            bool EnableSGConn = m_gemCfg.GPCollection[0].EnableComm;
            connString = connectionString;
            //"C:\\Users\\ariannakhor\\Desktop\\HostDataBase.mdb"
            //Microsoft.ACE.OLEDB.12.0
            //Microsoft.JET.OLEDB.4.0
            //Documents\\Projects\\GreatechApp-Prism_Platform_new_SecGemTest\\GreatechApp-Prism_Platform\\GreatechApp.Net\\GemDb
            connString = "Provider=Microsoft.ACE.OLEDB.12.0;data source=" + connectionString;
        }
        #endregion

        #region Method
        //public void RefreshConnectionString()
        //{

        //} 
        public void LoadSGConnection()
        {
            enablesgconn = EnableSGConn;
        }

        public void LoadConnProperties()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    conn.Open();

                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'DeviceID' ", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            deviceId = dr["ValueX"].ToString();
                        }
                     
                    }                                           

                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'IpadreessRemote' ", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            IpAddress = dr["ValueX"].ToString();
                        }
                    }

                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'PortRemote'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            portID = dr["ValueX"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
            }
        }
        public void LoadTimeOuts()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    conn.Open();

                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'T3'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            t3 = dr["ValueX"].ToString();
                        }
                    }
                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'T5'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            t5 = dr["ValueX"].ToString();
                        }
                    }
                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'T6'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            t6 = dr["ValueX"].ToString();
                        }
                    }
                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'T7'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            t7 = dr["ValueX"].ToString();
                        }
                    }
                    using (OleDbCommand cmd = new OleDbCommand("SELECT ValueX FROM Setting WHERE VariableX = 'T8'", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            t8 = dr["ValueX"].ToString();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public string ReadCEID_FromName(string evt_name)
        {
            string ValueX;
            OleDbConnection conn = new OleDbConnection(connString);

            conn.Open();
            string sql = "SELECT id FROM CEID WHERE evt_name ='" + evt_name + "'";
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            ValueX = ((int)cmd.ExecuteScalar()).ToString();
            conn.Close();

            return ValueX;
        }

        public string ReadVIDValue(string var_name)
        {
            string ValueX;
            OleDbConnection conn = new OleDbConnection(connString);

            conn.Open();
            string sql = "SELECT var_val FROM VID WHERE var_name ='" + var_name + "'";
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            ValueX = (String)cmd.ExecuteScalar();
            conn.Close();

            return ValueX;
        }
        public string ReadTableValue(string Table, string Target, string WhereX, string WhereY)
        {
            string ValueX;
            OleDbConnection conn = new OleDbConnection(connString);

            conn.Open();
            string sql = "SELECT " + Target + " FROM " + Table + " WHERE " + WhereX + " ='" + WhereY + "'";
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            ValueX = (String)cmd.ExecuteScalar();
            conn.Close();

            return ValueX;
        }

        public void UpdateVIDtoTable(string Value, int VID)
        {
            OleDbConnection conn = new OleDbConnection(connString);

            conn.Open();
            string sql = "UPDATE VID SET var_val='" + Value + "' WHERE id=" + VID.ToString();
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        #endregion

    }
}
