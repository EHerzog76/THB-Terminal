using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Core
{
    public class IPClass
    {
        private IPAddress _ip;
        private IPAddress _ipnetmask;
        private IPAddress _ipbcast;
        private IPAddress _ipnet;

        public IPClass(string ip, string NetMask)
        {
            _ip = IPAddress.Parse(ip);
            _ipnetmask = IPAddress.Parse(NetMask);

            _ipbcast = GetBroadcastAddress(_ip, _ipnetmask);
            _ipnet = GetNetworkAddress(_ip, _ipnetmask);
        }

        public string IP
        {
            get { return (_ip.ToString()); }
        }
        public string SubnetMask
        {
            get { return (_ipnetmask.ToString()); }
        }
        public string IPNet
        {
            get { return (_ipnet.ToString()); }
        }
        public string IPBCast
        {
            get { return (_ipbcast.ToString()); }
        }

        public IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public string Add2IP(string ip, long Offset)
        {
            long IPValue = IPToLong(ip);
            IPValue = IPValue + Offset;

            return (LongToIP(IPValue));
        }

        public bool IsInSameSubnet(IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            //IPAddress network1 = address.GetNetworkAddress(subnetMask);
            //IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            //return network1.Equals(network2);
            return (false);
        }

        public long IPToLong(string ip)
        {
            string[] ipBytes;
            double num = 0;
            if (!string.IsNullOrEmpty(ip))
            {
                ipBytes = ip.Split('.');
                for (int i = ipBytes.Length - 1; i >= 0; i--)
                {
                    num += ((int.Parse(ipBytes[i]) % 256) * Math.Pow(256, (3 - i)));
                }
            }
            return (long)num;
        }

        public string LongToIP(long longIP)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int)(longIP / Math.Pow(256, (3 - i)));
                longIP = longIP - (long)(num * Math.Pow(256, (3 - i)));
                if (i == 0)
                    ip = num.ToString();
                else
                    ip = ip + "." + num.ToString();
            }
            return ip;
        }
    }
}
