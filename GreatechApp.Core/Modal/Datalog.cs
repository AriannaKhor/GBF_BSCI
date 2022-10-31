using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System;
using System.Globalization;

namespace GreatechApp.Core.Modal
{
    public class Datalog: BindableBase
    {
        public Datalog(LogMsgType msgType, string descrip)
        {
            DateTime currentTime = DateTime.Now;
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "dd-MM-yyyy";
            Date = currentTime.ToString("d", dateFormat);
            Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            MsgType = msgType;
            MsgText = descrip;
        }

        public string Date { get; set; }
        public string Time { get; set; }
        public LogMsgType MsgType { get; set; }
        public string MsgText { get; set; }
    }
}
