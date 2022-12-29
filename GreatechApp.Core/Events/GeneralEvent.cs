namespace GreatechApp.Core.Events
{
    using GreatechApp.Core.Enums;
    using GreatechApp.Core.Modal;
    using Prism.Events;
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class TopVisionImage : PubSubEvent<BitmapImage>
    {

    }
    public class CodeReaderImage : PubSubEvent<BitmapImage>
    {

    }
    public class OnCodeReaderEndResultEvent : PubSubEvent
    {

    }
    public class OnCodeReaderDisconnectedEvent : PubSubEvent
    {

    }
    public class OnCodeReaderConnectedEvent : PubSubEvent
    {

    }
    public class RequestCodeReaderConnectionEvent : PubSubEvent
    {

    }
    public class RequestVisionLiveViewEvent: PubSubEvent
    {

    }

    public class RequestVisionConnectionEvent : PubSubEvent
    {

    }
    public class VisionConnectionEvent : PubSubEvent
    {

    }

    public class TopVisionResultEvent : PubSubEvent
    {
    }
       
    public class GetLiveImage : PubSubEvent
    {

    }
    public class GetLiveVideo : PubSubEvent
    {

    }
    public class StartGetLiveVideo : PubSubEvent
    {

    }
    public class StartGetLiveImage : PubSubEvent
    {

    }
    public class FormCloseConnection : PubSubEvent
    {

    }
    public class ValidateLogin : PubSubEvent<bool>
    {

    }
    public class ReceivedHostMsg : PubSubEvent<string>
    {

    }
    public class SecsGemCntrlState : PubSubEvent<string>
    {

    }

    
    public class MachineState : PubSubEvent<MachineStateType>
    {

    }

    public class CheckOperation: PubSubEvent<bool>
    {

    }

    public class MachineOperation : PubSubEvent<SequenceEvent>
    {

    }

    public class TestRunEvent : PubSubEvent<TestRunEvent>
    {
        public SQID SeqName;
        public TestRunEnum.SN TestRunSeq;
        public int TestRunCycle;
        public int MtrIdx;
    }

    public class TestRunResult: PubSubEvent<TestRunResult>
    {
        public bool result;
        public string ErrMsg;
    }

    public class OEEDataUpdate : PubSubEvent<OEEDataUpdate>
    {
        public int TotalInput;
        public int TotalOutput;
    }

    public class RefreshTotalInputOutput : PubSubEvent
    {

    }

    public class RefreshOEE : PubSubEvent<RefreshOEE>
    {
        public DateTime OEEStartTime;
        public Dictionary<int, int> OutputDict;
        public Dictionary<int, int> OutputShiftDict;
        
    }

    public class MachineStatusColor : PubSubEvent<MachineStatusColor>
    {
        public string Time;
        public SolidColorBrush StatusColor;
    }

    public class RefreshToolLife : PubSubEvent
    {

    }

    public class TCPIPStatus : PubSubEvent<TCPIPStatus>
    {
        public NetworkDev TCPDevice;
        public string IpAddress;
        public int Port;
        public bool IsAlive;
    }

    public class TCPIPMsg : PubSubEvent<TCPIPMsg>
    {
        public NetworkDev TCPDevice;
        public string IpAddress;
        public int Port;
        public byte[] MessageByte;
        public string Message;
    }
    
    public class ErrorMsg : PubSubEvent<ErrorMsg>
    {
        public string[] ErrMsg;
        public SQID Module;
    }

    public class SeqStatusEvent : PubSubEvent<SeqStatus>
    {

    }

    public class DatalogEntity : PubSubEvent<DatalogEntity>
    {
        public LogMsgType MsgType { get; set; }
        public string MsgText { get; set; }

        public string DisplayView { get; set; }
    }

    public class ResultlogEntity : PubSubEvent<ResultlogEntity>
    {
        public LogMsgType MsgType { get; set; }
        public string MsgText { get; set; }

        public string DisplayView { get; set; }
    }

    public class Resultlog : PubSubEvent<ResultsDatalog>
    {

    }

    public class PerformanceEntity : PubSubEvent<PerformanceEntity>
    {
        public double UPH { get; set; }
        public double CycleTime { get; set; }
    }

    public class PerformanceCompact : PubSubEvent<PerformanceCompact>
    {
        public string OverallYield { get; set; }
        public string Throughput { get; set; }
        public string StopPages { get; set; }
        public string StartTime { get; set; }
        public string ElapsedTime { get; set; }
        public string DownTime { get; set; }
        public string LotStartTime { get; set; }
        public string LotElapsedTime { get; set; }
        public string LotFinishTime { get; set; }
        public string MTBA { get; set; }
        public string MTTA { get; set; }
        public string MTTR { get; set; }
        public string MTBF { get; set; }
    }

    public class UIEvent : PubSubEvent<UIEvent>
    {
        public SQID SeqID;
        public EventArgs EvArgs;
    }

    public class RecordDesignerItem : PubSubEvent
    {

    }

    public class UnitDataTranfer : PubSubEvent<UnitDataTranfer>
    {
        public SQID TargetSeq;
        public int TargetSlotIndex;
    }

    public class RefreshMarkerLayout : PubSubEvent
    {

    }

    public class SwapPosition : PubSubEvent<SwapPosition>
    {
        public SQID Seq1;
        public SQID Seq2;
    }

    public class InitOperation : PubSubEvent
    {

    }

    public class EndLotOperation : PubSubEvent
    {

    }

    public class CultureChanged : PubSubEvent
    {

    }

    public class EStopOperation : PubSubEvent
    {

    }

    public class WarningMsgOperation : PubSubEvent<WarningMsgOperation>
    {
        public string Station;
        public int ErrorCode;
    }

    public class WarningStatusCheck : PubSubEvent
    {

    }

    public class UpdateDatabase : PubSubEvent
    {

    }
}
