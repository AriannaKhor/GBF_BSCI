using AdvanTechAPI;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOManager
{
    public class AdvantechIO : BaseIO, IAdvantechIO
    {
        private AdvantechAPI m_AdvantechAPI;

        public AdvantechIO()
        {
            m_AdvantechAPI = new AdvantechAPI();

            MaxBitPerDevice = m_AdvantechAPI.MaxBitPerDevice;
        }

        public override void StartScanIO()
        {
            m_AdvantechAPI.StartScanIO();
        }

        public override bool OpenDevice()
        {
            foreach(bool conn in m_AdvantechAPI.IsIOCardConnected)
            {
                if (!conn)
                {
                    ErrorMsg = "Connect Advantech IO Fail\r\n";
                    return false;
                }
            }

            return true;
        }

        public override bool ReadBit(int bit)
        {
            int deviceIndex = bit / MaxBitPerDevice;
            int bitIndex = bit % MaxBitPerDevice;

            return m_AdvantechAPI.InputBitStatus[deviceIndex,bitIndex].ToString() == "1";
        }

        public override bool ReadOutBit(int bit)
        {
            int deviceIndex = Convert.ToInt32(bit) / MaxBitPerDevice;
            int bitIndex = Convert.ToInt32(bit) % MaxBitPerDevice;

            return m_AdvantechAPI.OutputBitStatus[deviceIndex,bitIndex].ToString() == "1";
        }

        public override bool WriteBit(int? bit, bool state)
        {
            if(bit == null)
            {
                return false;
            }

            int deviceIndex = Convert.ToInt32(bit) / MaxBitPerDevice;
            int bitIndex = Convert.ToInt32(bit) % MaxBitPerDevice;

            return m_AdvantechAPI.TriggerOutput(deviceIndex, bitIndex, Convert.ToByte(state));
        }

    }
}
