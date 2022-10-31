namespace GreatechApp.Services.Utilities
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Net.NetworkInformation;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.OleDb;
    using System.Collections;

    public class Converter
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        public static double ConvertCurrentPos(double Value, string ConversionType, string ConversionValue)
        {
            if (ConversionType == "*")
            {
                Value *= Convert.ToInt32(ConversionValue);
            }
            else if (ConversionType == "/")
            {
                Value /= Convert.ToInt32(ConversionValue);
            }

            return Value;
        }

        public static ObservableCollection<string> GetIPNetworkList()
        {
            ObservableCollection<string> NetworkList = new ObservableCollection<string>();
            NetworkInterface[] ifaceList = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface iface in ifaceList)
            {
                if (iface.OperationalStatus == OperationalStatus.Up && iface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    NetworkList.Add(iface.Name);
                }
            }
            return NetworkList;
        }

        public static string GetIPAddress(string NetworkInterfaceName)
        {
            string localIP = "";
            foreach (NetworkInterface netif in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netif.Name == NetworkInterfaceName)
                {
                    IPInterfaceProperties properties = netif.GetIPProperties();
                    foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                    {
                        localIP = unicast.Address.ToString();
                    }
                    break;

                }
            }
            return localIP;
        }

        public static string GetNetworkName(string IPAddress)
        {
            string networkName = "";
            foreach (NetworkInterface netif in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties properties = netif.GetIPProperties();
                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.ToString() == IPAddress)
                    {
                        networkName = netif.Name;
                        break;
                    }

                }
            }
            return networkName;
        }

        public static ArrayList[] ReadFromExcel(ArrayList[] OPCAddress)
        {
            string excelFilePath = @"..\Config Section\OPCAddress\OPCAddressList.xlsx";

            using (OleDbConnection connection =
                    new OleDbConnection((excelFilePath.TrimEnd().ToLower().EndsWith("x"))
                    ? "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + excelFilePath + "';" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'"
                    : "provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + excelFilePath + "';Extended Properties=Excel 8.0;"))
            {
                connection.Open();
                DataTable dt = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                OPCAddress = new ArrayList[dt.Rows.Count];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["TABLE_NAME"].ToString().Contains("$"))
                    {
                        OPCAddress[i] = new ArrayList();

                        //Load the DataTable with Sheet Data
                        OleDbCommand oconn = new OleDbCommand("select * from [" + dt.Rows[i]["TABLE_NAME"].ToString() + "]", connection);
                        OleDbDataAdapter adp = new OleDbDataAdapter(oconn);
                        DataTable dt1 = new DataTable();
                        adp.Fill(dt1);

                        for (int j = 0; j < dt1.Rows.Count; j++)
                        {
                            if (!Convert.IsDBNull(dt1.Rows[j].ItemArray[0]))
                            {
                                OPCAddress[i].Add(dt1.Rows[j].ItemArray[2].ToString() + dt1.Rows[j].ItemArray[3]);
                            }
                        }  
                    }
                }

                connection.Close();
            }
            return OPCAddress;
        }

        public static Type GetClassAssembly(string Path, string ClassName)
        {
            Type[] ClassList = Assembly.LoadFrom(Path).GetTypes();

            return ClassList[Array.FindIndex(ClassList, x => x.Name == ClassName)]; ;
        }
    }
}
