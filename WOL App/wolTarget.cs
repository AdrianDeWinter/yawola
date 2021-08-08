using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace WOL_App
{
    public struct WolTarget
    {
        public string Address { get; private set; }
        public string Port { get; private set; }
        public string Mac_string { get; private set; }
        public byte[] Mac { get; private set; }
        public HostName HostName { get; private set; }

        public string Name { get; private set; }

        public WolTarget(string address, string mac, string name, string port = "0")
        {
            Address = "";
            HostName = null;
            Port = port;
            Mac = new byte[102];
            Mac_string = "";
            Name = name;
            SetAddress(address);
            SetMac(mac);
        }
        public void SetAddress(string address)
        {
            Address = address;
            HostName = new HostName(address);
        }

        public void SetMac(string mac)
        {
            Mac_string = mac;
            //split the mc address string into its byte strings
            string[] macArray = mac.Split(':');

            Mac = new byte[6];

            //parse the mac's byte strings into a bytes in an array
            for (int i = 0; i < 6; i++)
                Mac[i] = byte.Parse(macArray[i], System.Globalization.NumberStyles.HexNumber);
        }

        public void SetPort(string port)
        {
            Port = port;
        }

        public byte[] MagicPacket()
        {
            byte[] packet = new byte[102];

            //wite the sync stream into the packet, and parse the mac's byte strings into a bytes in an array
            for (int i = 0; i < 6; i++)
                packet[i] = 0xff;

            //write 16 copies of the mac address into the packet
            for (int i = 6; i < 102; i += 6)
                for (int j = 0; j < 6; j++)
                    packet[i + j] = Mac[j];

            return packet;
        }

        public override string ToString()
        {
            return "Display Name: " + Name + "\nAddress: " + Address + "\nMAC: " + Mac_string;
        }
    }
}
