using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemManager
{
   public class GemMsgQue
    {

        //Msg Que Declaration
        MessageQueue QueueSend = new MessageQueue();

        private string ReceivePath = ".\\Private$\\Receive";
        private string SendPath = ".\\Private$\\Send";

        public delegate void QueHandler(string label, string body);
        public event QueHandler ReceiveQueMessage;

        //Msg Que Declaration
        MessageQueue QueueReceive = new MessageQueue();

        public GemMsgQue()
        {
            CheckAndCreateQue(QueueReceive, ReceivePath);
            QueueReceive = new MessageQueue(ReceivePath);
            QueueReceive.Formatter = new ActiveXMessageFormatter();
            QueueReceive.ReceiveCompleted += QueueReceive_ReceiveCompleted;
        }

        public void Start()
        {
            QueueReceive.BeginReceive();
        }

        private void QueueReceive_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            CaptureMessage(QueueReceive, e, out string Label, out string Body);//Read Que Message
            ReceiveQueMessage(Label, Body);// fire Event
        }

        private void CheckAndCreateQue(MessageQueue Que, string QuePath)
        {
            //Check Que-----------------------------------------------**
            if (!System.Messaging.MessageQueue.Exists(QuePath))
            {
                MessageQueue.Create(QuePath);
                Que = new MessageQueue(QuePath);
            }
        }

        private void CaptureMessage(MessageQueue MsgQue, ReceiveCompletedEventArgs e, out string Label, out string Body)
        {
            Label = "";
            Body = "";

            MsgQue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            try
            {
                var msg = MsgQue.EndReceive(e.AsyncResult);
                var MsgLabel = (string)msg.Label;
                var MsgBody = (string)msg.Body;

                Label = MsgLabel;
                Body = MsgBody;
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }

            MsgQue.BeginReceive();
        }

        public bool SendMessage(string Label, string Body)
        {
            ///Status Msg Send default as False
            bool StatusSending = false;

            try
            {
                System.Messaging.Message msg = new System.Messaging.Message();
                msg.Label = Label;
                msg.Body = Body;

                //Check Que already have or need to create
                if (!System.Messaging.MessageQueue.Exists(SendPath))
                {
                    MessageQueue.Create(SendPath);
                }

                QueueSend = new MessageQueue(SendPath);
                QueueSend.Send(msg);
                StatusSending = true;//Sending is Sucessful
            }

            catch
            {
                StatusSending = false;//Sending is Failed
            }

            ///Return Status Message Sending
            return StatusSending;
        }
    }
}
