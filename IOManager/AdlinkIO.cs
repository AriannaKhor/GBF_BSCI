using APS_Define_W32;
using APS168_W64;
using IOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MotionTest.IO_Manager
{
    public class AdlinkIO : BaseIO, IAdlinkIO
    {
        //I/O PARAM
        Int32 Port_No = 0;

        int TotalInputChannel = 0;
        int TotalOutputChannel = 0;
        private int m_ModuleNo;
        private int m_SubModuleNo;
        private int m_Board_ID;

        //Device PARAM
        Int32 Bus_No = 0;
        Int32 Card_Name = 0;
        Int32 StartAxisID = 0;
        Int32 TotalAxisNum = 0;
        Int32 Channel = 0;
        private int LiveStatus = 0; // return 33 if is connected
        private int Board_id_Error = -1;

        bool IsCardInit = false;
        internal bool[,] ReadInputState;
        internal bool[,] ReadOutputState;
        bool DIStatus;
        bool DOStatus;
        bool IsSlaveLive = false;

        private enum InitMode 
        {
            AutoInit,
        }

        private enum AdlinkReturnValue
        {
            True,
            LiveTrue = 33,
        }

        private enum AdlinkReturnBit
        {
            NaN = -1,
            False = 0,
            True,
        }

        public bool Initialise()
        {
            try
            {
#if !Simulation
                int board_id = 0;
                // Card(Board) initial
                bool _init = APS168.APS_initial(ref board_id, (int)InitMode.AutoInit) == (int)AdlinkReturnValue.True ? true : false; //result in bit example, if only BoardID = 0, then refereance param will return 1 if BoradID = 0 and 4 then will be 1001.
                return _init;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool Closing()
        {
            try
            {
#if !Simulation
                // Card(Board) initial
                bool _Closedevice = APS168.APS_close() == (int)AdlinkReturnValue.True ? true : false;
                return _Closedevice;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool OpenDevice()
        {
#if !Simulation
            if (Initialise())
            {
                return OpenADLinkIO();
            }
            else
            {
                return false;
            }
#else
            return true;
#endif
        }

        public override bool CloseDevice()
        {
#if !Simulation
            return CloseADLinkIO();
#else
            return true;
#endif
        }

        public bool ScanDevice()
        {
#if !Simulation
            return ScanADLinkIO();
#else
            return true;
#endif
        }

        public override bool ReadBit(int bit)
        {
#if !Simulation
            return ADLinkIOInputState(bit);
#else
            return false;
#endif
        }

        public bool ReadBit(ushort? iNum, bool invert)
        {
#if !Simulation
            if (invert)
            {
                return !ADLinkIOInputState((int)iNum);
            }
            else
            {
                return ADLinkIOInputState((int)iNum);
            }
#else
            return false;
#endif
        }

        public override bool ReadOutBit(int bit)
        {
#if !Simulation
            return ADLinkIOOutputState((int)bit);
#else
            return false;
#endif
        }

        public override bool WriteBit(int? bit, bool state)
        {
#if !Simulation
            if (bit == null)
            {
                return false;
            }

            SetADLinkOutput(Convert.ToUInt16(bit), Convert.ToUInt16(state));
            return true;
#else
            return false;
#endif
        }

        public bool SlaveStatus(int mod_No)
        {
#if !Simulation
            IsSlaveLive = APS168_W64.APS168.APS_get_slave_online_status(Board_ID, Bus_No, mod_No, ref LiveStatus) == (int)AdlinkReturnValue.True ?
                LiveStatus == (int)AdlinkReturnValue.LiveTrue : false ? true : false;

            return IsSlaveLive;
#else
            return false;
#endif
        }

        public int ModuleNo
        {
            get { return m_ModuleNo; }
            set { m_ModuleNo = value; }
        }

        public int SubModuleNo
        {
            get { return m_SubModuleNo; }
            set { m_SubModuleNo = value; }
        }

        public int Board_ID
        {
            get { return m_Board_ID; }
            set { m_Board_ID = value; }
        }

        bool OpenADLinkIO()
        {
            try
            {
                Int32 card_name = 0;
                Int32 startAxisID = 0;
                Int32 totalAxisNum = 0;
                int ret;

                bool IsOpenDeviceSuccess = true;

                APS168.APS_get_card_name(Board_ID, ref card_name);

                APS168.APS_get_first_axisId(Board_ID, ref startAxisID, ref totalAxisNum);

                //----------------------------------------------------
                Card_Name = card_name;
                TotalAxisNum = totalAxisNum;
                StartAxisID = startAxisID;

                if (TotalAxisNum == 4) Channel = 2;
                else if (TotalAxisNum == 8) Channel = 4;
                //----------------------------------------------------

                ret = APS168.APS_scan_field_bus(Board_ID, Bus_No);

                if (ret == (int)AdlinkReturnValue.True)
                {
                    ret = APS168.APS_start_field_bus(Board_ID, Bus_No, StartAxisID);
                    if (ret != (int)AdlinkReturnValue.True)
                    {
                        IsOpenDeviceSuccess = false;
                        MessageBox.Show("Start field bus error " + ret.ToString());
                    }
                    else
                    {
                        IsOpenDeviceSuccess = true;
                        //MessageBox.Show("Start field bus success ");
                    }
                }
                else
                {
                    IsOpenDeviceSuccess = false;
                    MessageBox.Show("Scan field bus fail! " + ret.ToString());
                }

                if (Board_ID == Board_id_Error)
                {
                    IsOpenDeviceSuccess = false;
                    MessageBox.Show("Board Id search error!");
                }

                return IsOpenDeviceSuccess;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private bool CloseADLinkIO()
        {
            try
            {
                bool IsCloseDeviceSuccess = true;

                int ret;

                //Stop field bus
                ret = APS168.APS_stop_field_bus(Board_ID, Bus_No);
                if (ret != (int)AdlinkReturnValue.True)
                {
                    MessageBox.Show("Stop field bus fail." + ret.ToString());
                    IsCloseDeviceSuccess = false;
                }
                else
                {
                    MessageBox.Show("Stop field bus successfully!");
                    IsCloseDeviceSuccess = true;
                }

                IsCardInit = false;

                return IsCloseDeviceSuccess;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

        }

        private bool ScanADLinkIO()
        {
            try
            {
                int ret = APS168.APS_scan_field_bus(Board_ID, Bus_No);
                bool IsScan = ret == (int)AdlinkReturnValue.True ? true : false;

                if (!IsScan)
                {
                    MessageBox.Show("Scan Device Fail (Error Code: " + ret.ToString() + ")");
                }
                return IsScan;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

        }

        private bool ADLinkIOInputState(int InputCh_No)
        {
            try
            {
                //if (SubModuleNo != 0)
                //{
                #region With SubModule

                //UInt32 DI_Value = 0;
                //ReadInputState = new bool[TotalInputChannel, TotalInputChannel];

                //APS168.APS_get_field_bus_od_data(Board_ID, Bus_No, ModuleNo, SubModuleNo, InputCh_No, ref DI_Value);
                //DIStatus = DI_Value == 1 ? true : false;
                #endregion

                //}
                //else
                //{

                #region Without SubModule
                Int32 DI_Value = (int)AdlinkReturnBit.False;
                ReadInputState = new bool[TotalInputChannel, DI_Value];

                APS168.APS_get_field_bus_d_channel_input(Board_ID, Bus_No, ModuleNo, InputCh_No, ref DI_Value);
                DIStatus = DI_Value == (int)AdlinkReturnBit.True ? true : false;
                #endregion

                //}

                return DIStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

        }

        private bool ADLinkIOOutputState(int OutputCh_No)
        {
            try
            {
                //if (SubModuleNo != 0)
                //{

                #region With SubModule
                //UInt32 DO_Value = 0;
                //ReadOutputState = new bool[TotalOutputChannel, DO_Value];

                //APS168.APS_get_field_bus_od_data(Board_ID, Bus_No, ModuleNo, SubModuleNo, OutputCh_No, ref DO_Value);
                //DOStatus = DO_Value == 1 ? true : false;
                #endregion

                //}
                //else
                //{

                #region Without SubModule
                Int32 DO_Value = (int)AdlinkReturnBit.False;
                ReadOutputState = new bool[TotalOutputChannel, DO_Value];

                APS168.APS_get_field_bus_d_channel_output(Board_ID, Bus_No, ModuleNo, OutputCh_No, ref DO_Value);
                DOStatus = DO_Value == (int)AdlinkReturnBit.True ? true : false;
                #endregion

                //}

                return DOStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

        }

        private void SetADLinkOutput(int OutputCh_No, int Operation)
        {
            try
            {
                #region Without SubModule
                APS168.APS_set_field_bus_d_channel_output(Board_ID, Bus_No, ModuleNo, OutputCh_No, Operation);
                #endregion

                #region With SubModule
                //APS168.APS_set_field_bus_od_data(Board_ID, Bus_No, ModuleNo, SubModuleNo, OutputCh_No, Convert.ToUInt16(Operation));
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
