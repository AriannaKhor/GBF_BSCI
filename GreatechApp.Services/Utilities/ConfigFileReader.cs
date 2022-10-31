using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace GreatechApp.Services.Utilities
{
    public class ConfigFileReader
    {
        public static int capacity = 512;
        public static string Directory = @"..\Config Section\Setup\";
        private string filePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
        string key,
        string val,
        string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSection(string section, IntPtr keyValue,
            int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue,
            [In, Out] char[] value, int size, string filePath);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        public ConfigFileReader(string Sub_Directory_Path)
        {
            this.filePath = Directory + Sub_Directory_Path;
        }

        #region writeCSV
        public static void WriteCSVHeader(DirectoryInfo path, string data)
        {
            string filePath = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
            //var csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                path.Create();
                File.WriteAllText(filePath, data);
                //File.WriteAllText(filePath, Convert.ToString(csv));
            }
        }
        public static void WriteCSV(DirectoryInfo path, string data)
        {
            string filePath = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
            var csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                path.Create();
                File.WriteAllText(filePath, Convert.ToString(csv));
            }

            // Read lines from source file

            string[] arr = File.ReadAllLines(filePath);

            foreach (string prevData in arr)
            {
                var newLine = string.Format("{0}{1}", prevData, Environment.NewLine);
                csv.Append(newLine);
            }
            csv.Append(string.Format("{0},{1}", DateTime.Now.ToString("HH:mm:ss"), data));

            File.WriteAllText(filePath, Convert.ToString(csv));
        }

        public static void WriteCSV(DirectoryInfo path, string CSVname, string data)
        {

            string filePath = path + "\\" + CSVname + " " + DateTime.Now.ToString("(yyyyMMdd)") + ".csv";
            var csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                path.Create();
                File.WriteAllText(filePath, Convert.ToString(csv));
                csv.Append(CSVname + "\n\n");
            }

            // Read lines from source file
            string[] arr = File.ReadAllLines(filePath);

            foreach (string prevData in arr)
            {
                var newLine = string.Format("{0}{1}", prevData, Environment.NewLine);
                csv.Append(newLine);
            }
            csv.Append(string.Format("{0},{1}", DateTime.Now.ToString("HH:mm:ss"), data));

            File.WriteAllText(filePath, Convert.ToString(csv));
        }
        public static void WriteLaserCSV(DirectoryInfo path, string CSVname, string data)
        {
            string filePath = path + "\\" + CSVname.Replace("/", "") + ".csv";
            var csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                path.Create();
                File.WriteAllText(filePath, "");
                csv.Append(CSVname + "\n\n");
            }

            // Read lines from source file
            string[] arr = File.ReadAllLines(filePath);

            foreach (string prevData in arr)
            {
                var newLine = string.Format("{0}{1}", prevData, Environment.NewLine);
                csv.Append(newLine);
            }
            csv.Append(string.Format("{0}{1}", DateTime.Now.ToString("HH:mm:ss") + "\n", data));

            File.WriteAllText(filePath, Convert.ToString(csv));
        }
        #endregion

        #region readCSV
        public static void ReadCSV(DirectoryInfo path, ref string[] data)
        {
            string readPath = path + ".csv";
            data = File.ReadAllLines(readPath);
        }


        #endregion

        public void WriteString(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.filePath);
        }

        public void WriteInteger(string section, string key, int value)
        {
            string Data = Convert.ToString(value);
            WritePrivateProfileString(section, key, Data, this.filePath);
        }

        public void WriteFloat(string section, string key, double value)
        {
            string Data = Convert.ToString(value);
            WritePrivateProfileString(section, key, Data, this.filePath);
        }

        public void WriteBool(string section, string key, bool value)
        {
            string Data = "0";
            if (value)
            {
                Data = "1";
            }
            WritePrivateProfileString(section, key, Data, this.filePath);
        }

        public string ReadString(string section, string key, string Default)
        {
            StringBuilder SB = new StringBuilder(600);
            int i = GetPrivateProfileString(section, key, "", SB, 600, this.filePath);
            return (SB.Length > 0) ? SB.ToString() : Default;
        }

        public int ReadInteger(string section, string key, int Default)
        {
            string Data = Convert.ToString(Default);
            StringBuilder SB = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.filePath);
            string Data2 = (SB.Length > 0) ? SB.ToString() : Data;
            return Convert.ToInt32(Data2);
        }

        public float ReadFloat(string section, string key, float Default)
        {
            string Data = Convert.ToString(Default);
            StringBuilder SB = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.filePath);
            string Data2 = (SB.Length > 0) ? SB.ToString() : Data;
            return (float)Convert.ToDouble(Data2);
        }

        public string[] ReadSections()
        {
            // first line will not recognize if ini file is saved in UTF-8 with BOM 
            while (true)
            {
                char[] chars = new char[capacity];
                int size = GetPrivateProfileString(null, null, "", chars, capacity, this.filePath);

                if (size == 0)
                {
                    return null;
                }

                if (size < capacity - 2)
                {
                    string result = new String(chars, 0, size);
                    string[] sections = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return sections;
                }

                capacity *= 2;
            }
        }

        public string[] GetKeys(string category)
        {

            byte[] buffer = new byte[2048];

            GetPrivateProfileSection(category, buffer, 2048, filePath);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            return tmp;
        }

        public Dictionary<string, string[]> ReadAllText()
        {
            string[] Section = ReadSections();

            Dictionary<string, string[]> GetAllInfo = new Dictionary<string, string[]>();

            for (int i = 0; i < Section.Length; i++)
            {
                string[] Value = GetKeys(Section[i]);
                GetAllInfo.Add(Section[i], Value);
            }

            return GetAllInfo;
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
        public static DisplayAlarm ShowSeqAlarm;
    }

    public delegate void DisplayException(string Alm_Msg, string Module);
    public delegate void DisplayAlarm(string Alm_Msg, bool reset);
}
