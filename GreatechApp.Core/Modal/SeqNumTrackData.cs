using Prism.Mvvm;
using System;

namespace GreatechApp.Core.Modal
{
    public class SeqNumTrackData : BindableBase
    {
        public string SeqNum
        {
            get;
            set;
        }

        public string DateTime
        {
            get;
            set;
        }

        public string SeqName
        {
            get;
            set;
        }

        public SeqNumTrackData()
        {
        }

        public override string ToString()
        {
            char space = Convert.ToChar(" ");   // Provide alingment for the text
            return DateTime.PadRight(20, space) + SeqName.PadRight(25, space) + SeqNum;
        }
    }
}
