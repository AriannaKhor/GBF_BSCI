using ConfigManager;
using ConfigManager.Constant;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;
using IOManager;
using MotionManager;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Text;
using System.Windows.Forms;

namespace InterlockManager.Motion
{
    public class BaseMtrIntL : IMtrInterlock
    {
        protected static SystemConfig m_SystemConfig;
        protected static IBaseIO m_BaseIO;
        protected static IBaseMotion m_BaseMotion;
        public static IShowDialog m_ShowDialog;
        protected static CultureResources m_CultureResources;

        protected bool[] m_IntlChk = new bool[10];
        protected StringBuilder m_IntLMsg;
        protected static string m_Header;

        public BaseMtrIntL()
        {
            Provider = -1;
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_BaseIO = (IBaseIO)ContainerLocator.Container.Resolve(typeof(IBaseIO));
            m_BaseMotion = (IBaseMotion)ContainerLocator.Container.Resolve(typeof(IBaseMotion));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            m_IntLMsg = new StringBuilder();
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            m_Header = m_CultureResources.GetStringValue("Interlock") + " :\n";
            InitData();
        }


        public virtual bool CheckMtrInterlock(BaseCfg cfg, bool isSkipBusy = false)
        {
            InitData();
            m_Header = m_CultureResources.GetStringValue("Interlock") + " :\n";
            m_IntLMsg.Append(m_Header);

            MotionConfig motCfg = cfg as MotionConfig;

            if(motCfg == null)
            {
                return false;
            }

            if (m_SystemConfig.Machine.BypInterlock)
            {
                return true;
            }

            if(m_BaseMotion.AxisInMotion(motCfg.Axis.CardID, motCfg.Axis.AxisID) && !isSkipBusy)
            {
                m_IntLMsg.Append("- ").Append(motCfg.Axis.Name).Append(" : ").Append(m_CultureResources.GetStringValue("MotorMove")).AppendLine();
            }

            if(motCfg.Option.ChkAlarm)
            {
                bool alarm = m_BaseMotion.GetAlarmStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID);

                if((motCfg.Option.AlarmContact == Contact.NC && !alarm) || (motCfg.Option.AlarmContact == Contact.NO && alarm))
                {
                    m_IntLMsg.Append("- ").Append(motCfg.Axis.Name).Append(" : ").Append(m_CultureResources.GetStringValue("MotorAlarm")).AppendLine();
                }
            }

            if(!m_BaseMotion.GetServoStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID))
            {
                m_IntLMsg.Append("- ").Append(motCfg.Axis.Name).Append(" : ").Append(m_CultureResources.GetStringValue("MotorNotEnabled")).AppendLine();
            }

            // Check E-Stop button
            if (m_BaseIO.ReadBit((int)IN.DI0100_E_StopBtn, true))
            {
                m_IntLMsg.Append("- ").Append(m_CultureResources.GetStringValue("EStopNotRelease")).AppendLine();
            }

            if (Global.MachineStatus == MachineStateType.Running || Global.MachineStatus == MachineStateType.Initializing)
            {
                m_IntLMsg.Append("- ").Append(m_CultureResources.GetStringValue("SeqActive")).AppendLine();
            }

            // Always return false to allow child class continue to check for respective interlock.
            return false;
        }


        protected bool Finalize()
        {
            if (m_IntLMsg.ToString().CompareTo(m_Header) == 0)
            {
                return true;
            }

            m_ShowDialog.Show(DialogIcon.Error, m_CultureResources.GetStringValue("Interlock"), m_IntLMsg.ToString(), ButtonResult.OK);

            return false;
        }

        protected void InitData()
        {
            m_IntLMsg.Clear();
            for (int i = 0; i < 10; i++)
            {
                m_IntlChk[i] = false;
            }
        }

        public int Provider { get; set; }

    }
}
