using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace GreatechApp.Services.Utilities
{
    public class MachineBase
    {
        private static IEventAggregator m_Events = (IEventAggregator)ContainerLocator.Container.Resolve(typeof(IEventAggregator));
        public const string OffIcon = "/GreatechApp.Core;component/Icon/GrayIcon.png";
        public const string GreenOnIcon = "/GreatechApp.Core;component/Icon/Button-Blank-Green-icon.png";
        public const string RedOnIcon = "/GreatechApp.Core;component/Icon/Red-Ball-icon.png";
        public const string SwitchOnIcon = "/Resources/Images/switch-on.png";
        public const string SwitchOffIcon = "/Resources/Images/switch-off.png";
        public const bool DblSol = true;
        public const bool SkipIntLChk = true; // certain IO trigger such as tower lights & buzzer does not require interlock check

        public enum MessageMode
        {
            NonePopUpDialog,
            PopUpDialog
        }

        public enum MessageIcon
        {
            None,
            Error,
            Question,
            Warning,
            Information
        }

        public MachineBase(IEventAggregator events)
        {
        }

        public static void ShowMessage(string MessageDescription, MessageIcon messageIcon)
        {
            if (messageIcon == MessageIcon.Error)
            {
                System.Windows.Forms.MessageBox.Show(MessageDescription, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (MessageDescription.ToString().Contains("\r") || MessageDescription.ToString().Contains("\n") || MessageDescription.ToString().Contains(","))
                {
                    MessageDescription = MessageDescription.Replace('\r', '{');
                    MessageDescription = MessageDescription.Replace('\n', '}');
                    MessageDescription = MessageDescription.Replace(',', '|');
                }
                
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity
                {
                    MsgText = MessageDescription,
                    MsgType = LogMsgType.Error,
                });
            }

            else if (messageIcon == MessageIcon.Warning)
            {
                System.Windows.Forms.MessageBox.Show(MessageDescription, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity
                {
                    MsgText = MessageDescription,
                    MsgType = LogMsgType.Warning,
                });
            }

            else if (messageIcon == MessageIcon.Information)
            {
                System.Windows.Forms.MessageBox.Show(MessageDescription, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity
                {
                    MsgText = MessageDescription,
                    MsgType = LogMsgType.Info,
                });
            }
            
        }
        public static void ShowMessage(Exception MessageDescription)
        {
            string Msg = MessageDescription.ToString();

            System.Windows.Forms.MessageBox.Show(MessageDescription.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (MessageDescription.ToString().Contains("\r") || MessageDescription.ToString().Contains("\n") || MessageDescription.ToString().Contains(","))
            {

                Msg = Msg.Replace('\r', '{');
                Msg = Msg.Replace('\n', '}');
                Msg = Msg.Replace(',', '|');
            }
            
            m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity
            {
                MsgText = Msg,
                MsgType = LogMsgType.Error,
            });
        }
        public static void ShowMessage(AggregateException MessageDescription)
        {
            string Msg = MessageDescription.ToString();

            System.Windows.Forms.MessageBox.Show(MessageDescription.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (MessageDescription.ToString().Contains("\r") || MessageDescription.ToString().Contains("\n") || MessageDescription.ToString().Contains(","))
            {

                Msg = Msg.Replace('\r', '{');
                Msg = Msg.Replace('\n', '}');
                Msg = Msg.Replace(',', '|');
            }
            
            m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity
            {
                MsgText = Msg,
                MsgType = LogMsgType.Error,
            });
        }

        /// <summary>
        /// Calculate the device index number.
        /// ioNum will always have the fix Tag value, i.e. 0 - 15 / 0 - 32.
        /// The Max Bit Size (PortSize * NumOfPort) and Slave No (zero base index) 
        /// will be used to offset the Tag value
        /// </summary>
        /// <param name="ioNum"></param>
        /// <param name="slaveNo"></param>
        /// <returns>ushort</returns>
        
        /// <summary>
        /// Pad the specified character as the leading characters and trailing characters to the target string. 
        /// </summary>
        /// <param name="str">Target string that will be padded with the pad character.</param>
        /// <param name="finalStrSize">Final size size of the padded string.</param>
        /// <param name="padChar">Character to be used as the pad character.</param>
        /// <returns>Result string that has been padded.</returns>
        public static string Pad(string targetStr, int finalStrSize, char padChar)
        {
            string pad = string.Empty;
            try
            {
                StringBuilder sb = new StringBuilder();
                int totalPadSize = finalStrSize - targetStr.Length;

                if (totalPadSize < 0)
                {
                    throw new Exception("EXCEPTION: Size of the final string cannot be less than the size of the target string.");
                }
                else if (totalPadSize % 2 != 0)
                {
                    // The totalPadSize CANNOT be evenly distributed to Leading Pad and Trailing Pad.
                    int trailingPadSize = (int)Math.Ceiling((double)totalPadSize / 2);

                    for (int i = 0; i < trailingPadSize; i++)
                    {
                        sb.Append(padChar);
                    }

                    pad = sb.ToString();

                    sb = new StringBuilder(targetStr);
                    sb.Insert(0, pad.ToCharArray(0, trailingPadSize - 1));
                    sb.Append(pad);
                }
                else
                {
                    // The totalPadSize CAN be evenly distributed to Leading Pad and Trailing Pad.
                    for (int i = 0; i < totalPadSize / 2; i++)
                    {
                        sb.Append(padChar);
                    }

                    pad = sb.ToString();

                    sb = new StringBuilder(targetStr);
                    sb.Insert(0, pad);
                    sb.Append(pad);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                //ShowMessage(ex);
                return pad;
            }
        }
    }
}
