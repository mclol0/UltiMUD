using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Damdai
{
    public static class Net
    {
        public static string PublicIpAddress
        {
            get
            {
                try
                {
                    IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress ip in ipEntry.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            return ip.ToString();
                    }
                    if (ipEntry.AddressList.Length > 0)
                        return ipEntry.AddressList[0].ToString();
                }
                catch
                {
                }
                return IPAddress.Loopback.ToString();
            }
        }

        public static string GetInternalIp()
        {
            return PublicIpAddress;
        }

        public static string GetExternalIp()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");
                req.Timeout = 3000;
                req.ReadWriteTimeout = 3000;
                using (WebResponse resp = req.GetResponse())
                using (System.IO.Stream stream = resp.GetResponseStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                {
                    string ip = reader.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(ip))
                        return ip;
                }
            }
            catch
            {
            }
            return GetInternalIp();
        }
    }
}
