using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using IOManager;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Text;

namespace InterlockManager.IO
{
    public class BaseIOIntL /*: IIOInterlock*/
    {
        protected static SystemConfig m_SystemConfig;
        protected static IBaseIO m_BaseIO;
        //protected static IBaseMotion m_BaseMotion;
        public static IShowDialog m_ShowDialog;
        protected static CultureResources m_CultureResources;

        protected bool[] m_IntlChk = new bool[10];
        protected StringBuilder m_IntLMsg;
        protected static string m_Header;

        public BaseIOIntL()
        {
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_BaseIO = (IBaseIO)ContainerLocator.Container.Resolve(typeof(IBaseIO));
            //m_BaseMotion = (IBaseMotion)ContainerLocator.Container.Resolve(typeof(IBaseMotion));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            m_IntLMsg = new StringBuilder();
            m_Header = m_CultureResources.GetStringValue("Interlock") + " :\n";
            InitData();
        }
        public string GetSeqName(OUT ioNum)
        {
            SQID seqName;

            if(m_BaseIO.IOKeyList.TryGetValue(ioNum, out seqName))
            {
            }
            return seqName.ToString();
        }

   //     public virtual bool CheckIOInterlock(int ioNum, bool oState, bool isChildExist)
   //     {
   //         InitData();
   //         m_IntLMsg.Append(m_Header);

   //         if(m_SystemConfig.Machine.BypInterlock)
   //         {
   //             return true;
   //         }

   //         if(m_BaseIO.ReadBit((int)IN.DI0100_E_StopBtn, true))
   //         {
   //             m_IntLMsg.Append(" - ").Append(m_CultureResources.GetStringValue("EStopNotRelease")).AppendLine();
   //         }

   //         if(isChildExist)
			//{
   //             // Always return false to allow child class continue to check for respective interlock.
   //             return false;
   //         }
   //         else
			//{
   //             return Finalize();
			//}
   //     }

        protected bool Finalize() 
        {
            if(m_IntLMsg.ToString().CompareTo(m_Header) == 0)
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

        public SQID Provider { get; set; }

    }
}
