using System;
using System.Text;

namespace GreatechApp.Core.Helpers
{
    public class FileHelper
    {
        public static string Pad(string targetStr, int finalStrSize, char padChar)
        {
            string pad = string.Empty;
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
    }
}
