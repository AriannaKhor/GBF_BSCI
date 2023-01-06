using GreatechApp.Core.Enums;

namespace InterlockManager.IO
{
    public class SampleSeqIOIntL : BaseIOIntL
    {
        public SampleSeqIOIntL()
        {
            Provider = SQID.SampleSeq;
        }

        public override bool CheckIOInterlock(int ioNum, bool oState, bool isChildExist)
        {
            if(base.CheckIOInterlock(ioNum, oState, isChildExist))
            {
                return true;
            }

            #region Gripper Work - Extend
            //if((ioNum == (int)OUTList.DO0109_Test_GripperWork) && oState == true)
            //{
            //    m_IntlChk[0] = !m_BaseIO.ReadBit((int)INList.DI0104_Input5);

            //    if(m_IntlChk[0])
            //    {
            //        m_IntLMsg.Append("- Not Safe to trigger Gripper.").AppendLine().
            //                  Append("*** Please Move Input 5 to rest position. ***");
            //    }
            //}
            #endregion

            return Finalize();
        }
    }
}
