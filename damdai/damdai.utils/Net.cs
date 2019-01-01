using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace Damdai
{
    public static class Net
    {
        public static string PublicIpAddress
        {
            get
            {
                IPHostEntry ipEntry = Dns.GetHostByName(Dns.GetHostName());
                IPAddress[] ips = ipEntry.AddressList;
                return ips[0].ToString();
            }
        }

        public static string GetExternalIp()
        {
            string url = "http://www.whatismyip.com/automation/n09230945.asp";
            WebClient wc = new WebClient();
            UTF8Encoding utf8 = new UTF8Encoding();
            return utf8.GetString(wc.DownloadData(url));
        }
    }
}
