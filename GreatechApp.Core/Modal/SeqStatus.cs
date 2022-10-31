using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System;

namespace GreatechApp.Core.Modal
{
    public class SeqStatus : BindableBase
    {
        private bool m_IsSeqAlive = false;
        private string m_SeqName;
        private string m_SeqNum;

        public string SeqName
        {
            get { return m_SeqName; }
            set { SetProperty(ref m_SeqName, value); }
        }

        public string SeqNum
        {
            get { return m_SeqNum; }
            set { SetProperty(ref m_SeqNum, value); }
        }

        public bool IsSeqAlive 
        {
            get { return m_IsSeqAlive; }
            set { SetProperty(ref m_IsSeqAlive, value); }
        }

        public SeqStatus(SQID seqID)
        {
            char space = Convert.ToChar(" ");	// Provide alingment for the text

            SeqName = seqID.ToString().PadRight(25,space);
            SeqNum = "EOS".PadRight(25, space);
            IsSeqAlive = true;
        }

        public override string ToString()
        {
            char space = Convert.ToChar(" ");	// Provide alingment for the text
            return SeqName.PadRight(25, space) + SeqNum.PadRight(25, space) + IsSeqAlive.ToString();
        }
    }
}
