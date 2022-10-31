using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using TCPIPManager.Enums;

namespace TCPIPManager.Infrastructure
{
    public static class SupportMethod
    {
        #region Description.
        /// <summary>
        /// Show all exception message, including the inner exception, in a MessageBox.
        /// </summary>
        /// <param name="ex">Exception to show.</param>
        /// <param name="output">One of the SocketManager.Output values.</param>
        #endregion
        internal static void ShowExceptionMessage(Exception ex, Output output)
        {
            List<Exception> exList = new List<Exception>();
            StringBuilder errMsg = new StringBuilder();
            string caption = ex.TargetSite.DeclaringType.ToString() + "." + ex.TargetSite.Name.ToString() + "()";

            exList.Add(ex);

            #region Add all InnerException into exList.
            // ref: http://codepalace.blogspot.com/2008/06/how-to-get-all-nested-inner-exceptions.html
            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                exList.Add(innerEx);
                innerEx = innerEx.InnerException;
            }
            #endregion

            #region Build error message.
            foreach (Exception exp in exList)
            {
                string type = exp.GetType() == null ? "N/A" : exp.GetType().Name;
                string message = exp.Message == null ? "N/A" : exp.Message;
                string declaringType = exp.TargetSite.DeclaringType == null ? "N/A" : exp.TargetSite.DeclaringType.Name;
                string targetSite = exp.TargetSite == null ? "N/A" : exp.TargetSite.Name;
                string stackTrace = exp.StackTrace == null ? "N/A" : exp.StackTrace;

                errMsg.AppendLine("Type: " + type);
                errMsg.AppendLine("Message: " + message);
                errMsg.AppendLine("Declaring Type: " + declaringType);
                errMsg.AppendLine("Target Site: " + targetSite);
                errMsg.AppendLine("Stack Trace:\r\n " + stackTrace);
                errMsg.AppendLine();
                errMsg.AppendLine();
            }
            #endregion

            #region Display the error message in the selected window.
            switch (output)
            {
                case Output.MessageBox:
                    MessageBox.Show(
                        errMsg.ToString(),
                        caption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;

                case Output.Debug:
                    Debug.WriteLine(caption + "\n" + errMsg.ToString());
                    break;

                case Output.EventLog:
                    try
                    {
                        EventLog.WriteEntry(typeof(SupportMethod).Assembly.GetName().Name, errMsg.ToString(), EventLogEntryType.Error);
                    }
                    catch(Exception e)
                    {
                        ShowExceptionMessage(e, output);
                    }
                    break;
            }
            #endregion
        }
    }
}
