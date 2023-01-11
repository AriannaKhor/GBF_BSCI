using CsvHelper.Configuration;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Variable;
using Prism.Mvvm;
using System;
using System.Globalization;


namespace GreatechApp.Core.Modal
{
    public class Datalog : BindableBase
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

    public class ResultsDatalog : BindableBase
    {
        public ResultsDatalog()
        {
        }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Timestamp { get; set; }
        public string UserId { get; set; }
        public string UserLvl { get; set; }
        public string TopVision { get; set; }
        public string CodeReader { get; set; }
        public float VisTotalPrdQty { get; set; }
        public float VisCorrectOrient { get; set; }
        public float VisWrongOrient { get; set; }
        public int DecodeBatchQuantity { get; set; }
        public int DecodeBoxQuantity { get; set; }
        public int DecodeAccuQuantity { get; set; }
        public string OverallResult { get; set; }
        public string ErrorMessage { get; set; }
        public string VisErrorMessage { get; set; }
        public string CodeReaderErrorMessage { get; set; }
        public string Remarks { get; set; }
        public string ForceEndLotRemarks { get; set; }
        public string ApprovedBy { get; set; }
        public string ForceEndLotApprovedBy { get; set; }

        public void ClearAll()
        {
            UserId = string.Empty;
            UserLvl = string.Empty;

            TopVision = string.Empty;
            VisTotalPrdQty = 0;
            VisCorrectOrient = 0;
            VisWrongOrient = 0;

            CodeReader = string.Empty;
            DecodeBatchQuantity = 0;
            DecodeBoxQuantity = 0;
            DecodeAccuQuantity = 0;
            OverallResult = string.Empty;

            ErrorMessage = string.Empty;
            Remarks = string.Empty;
            ApprovedBy = string.Empty;
        }

        public sealed class ResultsDatalogMap : ClassMap<ResultsDatalog>
        {
            public ResultsDatalogMap()
            {
                Map(m => m.UserId).Name("User ID");
                Map(m => m.UserLvl).Name("User Level");
                Map(m => m.Timestamp).Name("Timestamp");
                Map(m => m.TopVision).Name("Inspection Type");
                Map(m => m.VisTotalPrdQty).Name("Total Quantity");
                Map(m => m.VisCorrectOrient).Name("Correct Orientation");
                Map(m => m.VisWrongOrient).Name("Wrong Orientation");
                Map(m => m.CodeReader).Name("Inspection Type");
                Map(m => m.DecodeBatchQuantity).Name("Decoded Batch Quantity");
                Map(m => m.DecodeBoxQuantity).Name("Decoded Box Quantity");
                Map(m => m.DecodeAccuQuantity).Name("Accumulated Batch Quantity");
                Map(m => m.OverallResult).Name("Overall Result");
                Map(m => m.ErrorMessage).Name("Messages");
                Map(m => m.Remarks).Name("Remarks");
                Map(m => m.ApprovedBy).Name("Approved By");
            }
        }
    }
}

