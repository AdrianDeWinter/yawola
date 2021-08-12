using System;
using Windows.Networking;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;

namespace WOL_App
{
    public class WolTarget
    {
        public string Address { get; private set; } = "";
        public string Port { get; private set; } = "";
        public string Mac_string { get; private set; } = "";
        public string Name { get; private set; } = "";

        private byte[] Mac { get; set; } = new byte[102];
        private HostName HostName { get; set; } = null;

        public WolTarget(string address, string mac, string name, string port = "0")
        {
            Name = name;
            SetAddress(address);
            SetPort(port);
            SetMac(mac);
        }
        public WolTarget(string address, string[] mac, string name, string port = "0")
        {
            Name = name;
            SetAddress(address);
            SetPort(port);
            ParseMac(mac);
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

            ParseMac(macArray);
        }
        private void ParseMac(string[] macSubstrings)
        {
            Mac = new byte[6];
            Mac_string = "";
            //parse the mac's byte strings into a bytes in an array
            for (int i = 0; i < 6; i++)
            {
                //if anything was entered into this field, parse it
                if (macSubstrings[i] != "")
                {
                    //if a single digit was entered into this field, pad with a zero
                    if (macSubstrings[i].Length < 2)
                        Mac_string += "0";
                    Mac_string += macSubstrings[i];
                    Mac[i] = byte.Parse(macSubstrings[i], System.Globalization.NumberStyles.HexNumber);
                }
                //if nothing was entered, default to "00"
                else
                {
                    Mac_string += "00";
                    Mac[i] = 0;
                }

                //also add colons to the mac string as it is being built (except after the last byte)
                if (i < 5)
                    Mac_string += ":";
            }
        }
        public void SetPort(string port)
        {
            Port = port;
        }
        private byte[] MagicPacket()
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
        public async void SendMagicPacket(bool debug = false)
        {
            if (debug)
            {
                Debug.WriteLine("Sending packet:");
                Debug.WriteLine(ToString());
            }

            DatagramSocket socket = new DatagramSocket();
            //get out stream to the selected target
            IOutputStream outStream;
            try
            {
                outStream = await socket.GetOutputStreamAsync(HostName, Port);
            }
            catch (Exception except)
            {
                Debug.WriteLine("Send failed with error: " + except.Message);
                return;
            }
            //obtain writer for the stream
            DataWriter writer = new DataWriter(outStream);
            //write the packet to the stream
            if (debug)
                Debug.WriteLine("writing data");
            writer.WriteBytes(MagicPacket());
            //send the packet
            try
            {
                _ = await writer.StoreAsync();
            }
            catch (Exception except)
            {
                Debug.WriteLine("Send failed with error: " + except.Message);
                return;
            }
            if (debug)
                Debug.WriteLine("Packet sent");
        }
        public override string ToString()
        {
            return "Display Name: " + Name + "\nAddress: " + Address + "\nMAC: " + Mac_string + "\nPort: " + Port;
        }
    }
}
