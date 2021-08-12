using System;
using Windows.Networking;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;
using System.Threading.Tasks;

namespace WOL_App
{
    /**
     * <summary>This class stores all information on an individual host, and provides functionality to interact with it
     * <para>It provides two, mostly identical, constructors, one accepts the mac as a colon delimited string (ie <c>12:34:56:78:9A:BC</c>),
     * and one that accepts an array of six, two character long, strings to represent each byte</para>
     * </summary>
     */
    public class WolTarget
    {
        /// <summary>
        /// The hosts address as entered in the input field. Should only be used for display porposes,<br/>
        /// if code intends to work with the actual information, the <see cref="HostName"/> member should be used instead.
        /// </summary>
        public string Address { get; private set; } = "";
        /// <summary>
        /// The udp port this host should be messaged on (the setter (<see cref="SetPort(string)"/>) and <see cref="MainPage.addHostDialog"/> enforce this to be a string of digits)
        /// </summary>
        public string Port { get; private set; } = "";
        /// <summary>
        /// The string representation of the hosts mac address, represented as a colon delimited string (ie <c>12:34:56:78:9A:BC</c>).<br/>
        /// Depending on the constructor used, this is either the original string passed to the constructor, or built from the array passed to the constructor.<br/>
        /// Should only be used for display porposes, if code intends to work with the actual information, the <see cref="HostName"/> member should be used instead.
        /// </summary>
        public string Mac_string { get; private set; } = "";
        /// <summary>
        /// A simple display name set by the user. Nothing fancy about this, should never be used in code since it is not cleaned in any way.
        /// </summary>
        public string Name { get; private set; } = "";

        /// <summary>
        /// The hosts mac address. Any code that wishes to work with the mac address should use this memeber (and do so within the class).<br/>
        /// If the mac address needs to be displayed, the <see cref="Mac_string"/> member should be used.
        /// </summary>
        /// <remarks>This member is deliberatly private, to ensure any code accessing the actual mac address of this object does so within the class, or uses the appropriate setter (<see cref="SetMac(string)"/>)</remarks>
        private byte[] Mac { get; } = new byte[6];
        /// <summary>
        /// The targets address, represented as an instance of <see cref="Windows.Networking.HostName"/>.<br/>
        /// When working with the targets address in code, this member should bes used.<br/>
        /// If the address needs to be displayed, the <see cref="Address"/> member should bes uesed instead.
        /// </summary>
        /// <remarks>This member is deliberatly private, to ensure any code accessing the actual address of this object does so within the class, or uses the appropriate setter (<see cref="SetAddress(string)"/>)</remarks>
        private HostName HostName { get; set; } = null;

        /// <summary>
        /// Creates a new instance of wolTarget. This overload accepts the mac address in the form of a colon delimited string..<br/>
        /// Another overload exists which takes an array of six two char long strings as the mac address: <seealso cref="WolTarget(string, string[], string, string)"/>
        /// </summary>
        /// <param name="address">the IPv4 address, IPv6 address, hostname or url for this host</param>
        /// <param name="mac">the mac address in the form <c>12:34:56:78:9A:BC</c></param>
        /// <param name="name">a simple display name, has no actual functionality</param>
        /// <param name="port">the udp port number to use. may only contain decimal digits</param>
        public WolTarget(string address, string mac, string name, string port = "0")
        {
            Name = name;
            SetAddress(address);
            SetPort(port);
            SetMac(mac);
        }
        /// <summary>
        /// Creates a new instance of wolTarget. This overload accepts the mac address in the form of an array of six two char long strings.<br/>
        /// Another overload exists which takes a colon delimited string as the mac address: <seealso cref="WolTarget(string, string, string, string)"/>
        /// </summary>
        /// <param name="address">the IPv4 address, IPv6 address, hostname or url for this host</param>
        /// <param name="mac">the mac address in the form of an array of six strings</param>
        /// <param name="name">a simple display name, has no actual functionality</param>
        /// <param name="port">the udp port number to use. may only contain decimal digits</param>
        public WolTarget(string address, string[] mac, string name, string port = "0")
        {
            Name = name;
            SetAddress(address);
            SetPort(port);
            ParseMac(mac);
        }
        /// <summary>
        /// Sets the <see cref="Address"/> and <see cref="HostName"/> members
        /// </summary>
        /// <param name="address">An IPv4 address, IPv6 address, hostName, or url</param>
        public void SetAddress(string address)
        {
            Address = address;
            HostName = new HostName(address);
        }
        /// <summary>
        /// Sets the mac address
        /// </summary>
        /// <param name="mac">a colon delimited string of with exactly six elements</param>
        /// <exception cref="ArgumentException">Thrown if the string did not conform to the correct format</exception>
        /// <exception cref="ArgumentNullException">Thrown if the string was null</exception>
        /// <exception cref="FormatException">Thrown if the string was not of the correct format.</exception>
        /// <exception cref="OverflowException">Thrown if a part of the string represents a number less than MinValue or greater than MaxValue or includes non-zero, fractional digits.</exception>
        public void SetMac(string mac)
        {
            Mac_string = mac;
            //split the mc address string into its byte strings
            string[] macArray = mac.Split(':');
            try
            {
                ParseMac(macArray);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Parses a mac represented as a hexadecimal string array into a byte array and sets the <see cref="Mac"/> and <see cref="Mac_string"/> members.
        /// </summary>
        /// <param name="macSubstrings">The mac address in the form of an array of six strings. Each string must be between zero and two chars long</param>
        /// <exception cref="ArgumentException">Thrown if the string did not conform to the correct format</exception>
        /// <exception cref="ArgumentNullException">Thrown if the string was null</exception>
        /// <exception cref="FormatException">Thrown if the string was not of the correct format.</exception>
        /// <exception cref="OverflowException">Thrown if a part of the string represents a number less than 0x0 or greater than 0xFF or includes non-zero, fractional digits.</exception>
        private void ParseMac(string[] macSubstrings)
        {
            if (macSubstrings.Length != 6)
                throw new ArgumentException("The input array must have exactly six elements");
            string new_Mac_string = "";
            byte[] new_mac = new byte[6];
            //parse the mac's byte strings into a bytes in an array
            for (int i = 0; i < 6; i++)
            {
                //if anything was entered into this field, parse it
                if (macSubstrings[i] != "")
                {
                    //if a single digit was entered into this field, pad with a zero
                    if (macSubstrings[i].Length < 2)
                        new_Mac_string += "0";
                    new_Mac_string += macSubstrings[i];
                    try
                    {
                        new_mac[i] = byte.Parse(macSubstrings[i], System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                //if nothing was entered, default to "00"
                else
                {
                    new_Mac_string += "00";
                    new_mac[i] = 0;
                }

                //also add colons to the mac string as it is being built (except after the last byte)
                if (i < 5)
                    new_Mac_string += ":";
            }

            //now that everything parsed successfully, set the new values
            Mac_string = new_Mac_string;
            for (int i = 0; i < 6; i++)
                Mac[i] = new_mac[i];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <exception cref="ArgumentNullException">Thrown if the string was null</exception>
        /// <exception cref="FormatException">Thrown if the string was not of the correct format.</exception>
        /// <exception cref="OverflowException">Thrown if a part of the string represents a number less than 0 or greater than 65535.</exception>
        public void SetPort(string port)
        {
            //parse the port string as an int to ensure it is a valid port number
            try
            {
                ushort p = ushort.Parse(port);
            }
            catch (Exception e)
            {
                throw e;
            }
            Port = port;
        }
        /// <summary>
        /// Builds an returns a magic packet for this host.
        /// </summary>
        /// <returns>A 102 byte array containing the udp payload for a magic packet to this host.</returns>
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
        /// <summary>
        /// Sends a magic pcket to wake the host
        /// </summary>
        /// <param name="debug">Wether debug information should be printed</param>
        /// <returns>true if the opration succeeded, false if any exceptions ocurred</returns>
        public async Task<bool> SendMagicPacket(bool debug = false)
        {
            if (debug)
            {
                Debug.WriteLine("Sending packet:");
                Debug.WriteLine(ToString());
            }

            DatagramSocket socket = new DatagramSocket();
            //get out stream to the selected target
            IOutputStream outStream = await socket.GetOutputStreamAsync(HostName, Port);

            //obtain writer for the stream
            DataWriter writer = new DataWriter(outStream);
            //write the packet to the stream
            if (debug)
                Debug.WriteLine("writing data");
            writer.WriteBytes(MagicPacket());
            if (await writer.StoreAsync() == 1)
            {
                if (debug)
                    Debug.WriteLine("Packet sent");
                return true;
            }
            else
            {
                if (debug)
                    Debug.WriteLine("Sending the packet failed");
                return false;
            }
        }
        /// <summary>
        /// An override of the Object.ToString method.
        /// </summary>
        /// <returns>A strign representation of this host</returns>
        public override string ToString()
        {
            return "Display Name: " + Name + "\nAddress: " + Address + "\nMAC: " + Mac_string + "\nPort: " + Port;
        }
    }
}
