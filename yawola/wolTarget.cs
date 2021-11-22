using System;
using Windows.Networking;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;
using System.Net.NetworkInformation;

namespace yawola
{
	///<summary>This class stores all information on an individual host, and provides functionality to interact with it.
	/// <para>It provides two, mostly identical, constructors, one accepts the mac as a colon delimited string (ie<c>12:34:56:78:9A:BC</c>),
	/// and one that accepts an array of six, two character long, strings to represent each byte</para>
	/// </summary>
	[Serializable]
	public class WolTarget : IXmlSerializable, INotifyPropertyChanged
	{
		/// <summary>
		/// The hosts address as entered in the input field. Should only be used for display porposes,<br/>
		/// if code intends to work with the actual information, the <see cref="HostName"/> member should be used instead.
		/// </summary>
		private string address;
		public string Address
		{
			get => address;
			set
			{
				if (value != address)
				{
					address = value;
					NotifyPropertyChanged();
				}
			}
		}
		/// <summary>
		/// The udp port this host should be messaged on (the setter (<see cref="SetPort(string)"/>) and <see cref="MainPage.addHostDialog"/> enforce this to be a string of digits)
		/// </summary>
		private string port;
		public string Port
		{
			get => port.Length != 0 ? port : ((int)AppData.GetSetting(AppData.Setting.defaultPort)).ToString();
			set
			{
			//TODO: would be better implmeneted as a init accessor, but that requires switching to C# 9.0+
				if (port == null)
					port = "";
				if (value != port)
				{
					//fall back to the configured default port number if an empty string was passed
					if (port.Length == 0) {
						dynamic defaultPortSetting = AppData.GetSetting(AppData.Setting.defaultPort);
						string defaultPortNumber = defaultPortSetting.GetType() == typeof(int) ? defaultPortSetting.ToString() : (string)defaultPortSetting;
						port = defaultPortNumber;
					}
					else
					{
						//parse the port string as an int to ensure it is a valid port number
						try
						{
							ushort p = ushort.Parse(port);
						}
						catch (Exception e)
						{
							return;
						}
						Port = port;
					}
					port = value;
					NotifyPropertyChanged();
				}
			}
		}
		/// <summary>
		/// The string representation of the hosts mac address, represented as a colon delimited string (ie <c>12:34:56:78:9A:BC</c>).<br/>
		/// Depending on the constructor used, this is either the original string passed to the constructor, or built from the array passed to the constructor.<br/>
		/// Should only be used for display porposes, if code intends to work with the actual information, the <see cref="HostName"/> member should be used instead.
		/// </summary>
		private string mac_string;
		public string Mac_string
		{
			get => mac_string;
			set
			{
				if (value != mac_string)
				{
					mac_string = value;
					NotifyPropertyChanged();
				}
			}
		}
		/// <summary>
		/// The hosts mac address as a string array. Any code that wishes to work with the mac address should use the <see cref="Mac"/> memeber (and do so within the class).<br/>
		/// If the mac address needs to be displayed, the <see cref="Mac_string"/> member should be used.
		/// </summary>
		public string[] Mac_string_array { get; private set; } = new string[6];

		/// <summary>
		/// A simple display name set by the user. Nothing fancy about this, should never be used in code since it is not cleaned in any way.
		/// </summary>
		private string name;
		public string Name
		{
			get => name;
			set
			{
				if (value != name)
				{
					name = value;
					NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// The hosts mac address. Any code that wishes to work with the mac address should use this memeber (and do so within the class).<br/>
		/// If the mac address needs to be displayed, the <see cref="Mac_string"/> member should be used.
		/// </summary>
		/// <remarks>This member is deliberatly private, to ensure any code accessing the actual mac address of this object does so within the class, or uses the appropriate setter (<see cref="SetMac(string)"/>)</remarks>
		private byte[] Mac = new byte[6];
		/// <summary>
		/// The targets address, represented as an instance of <see cref="Windows.Networking.HostName"/>.<br/>
		/// When working with the targets address in code, this member should bes used.<br/>
		/// If the address needs to be displayed, the <see cref="Address"/> member should bes uesed instead.
		/// </summary>
		/// <remarks>This member is deliberatly private, to ensure any code accessing the actual address of this object does so within the class, or uses the appropriate setter (<see cref="SetAddress(string)"/>)</remarks>
		private HostName HostName = null;

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.  
		// The CallerMemberName attribute that is applied to the optional propertyName  
		// parameter causes the property name of the caller to be substituted as an argument.  
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		}

		public WolTarget()
		{ Name = "defaul"; Address = "defaultAddress"; Port = "0"; Mac_string = "00:00:00:00:00:00"; }
		/// <summary>
		/// Creates a new instance of wolTarget. This overload accepts the mac address in the form of a colon delimited string..<br/>
		/// Another overload exists which takes an array of six two char long strings as the mac address: <seealso cref="WolTarget(string, string[], string, string)"/>
		/// </summary>
		/// <param name="address">the IPv4 address, IPv6 address, hostname or url for this host</param>
		/// <param name="mac">the mac address in the form <c>12:34:56:78:9A:BC</c></param>
		/// <param name="name">a simple display name, has no actual functionality</param>
		/// <param name="port">the udp port number to use. may only contain decimal digits</param>
		/// <exception cref="ArgumentException">Thrown if an invalid parameter was passed, see the exception message for mor info</exception>
		/// <exception cref="ArgumentNullException">Thrown if null was passed to a parameter, see the exception message for more info</exception>
		public WolTarget(string address, string mac, string name, string port = "9999")
		{
			if (name == null || name.Length == 0)
				throw new ArgumentException("The display name for a WolTarget cannot be empty");
			Name = name ?? throw new ArgumentNullException("The display name for a WolTarget cannot be null");
			try
			{
				SetAddress(address);
				Port = port;
				SetMac(mac);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		/// <summary>
		/// Creates a new instance of wolTarget. This overload accepts the mac address in the form of an array of six two char long strings.<br/>
		/// Another overload exists which takes a colon delimited string as the mac address: <seealso cref="WolTarget(string, string, string, string)"/>
		/// </summary>
		/// <param name="address">the IPv4 address, IPv6 address, hostname or url for this host</param>
		/// <param name="mac">the mac address in the form of an array of six strings</param>
		/// <param name="name">a simple display name, has no actual functionality</param>
		/// <param name="port">the udp port number to use. may only contain decimal digits</param>
		/// <exception cref="ArgumentException">Thrown if an invalid parameter was passed, see the exception message for more info</exception>
		/// <exception cref="ArgumentNullException">Thrown if null was passed to a parameter, see the exception message for more info</exception>
		public WolTarget(string address, string[] mac, string name, string port = "9999")
		{
			//evaluate the name string, throw appropriate exception if necessary
			if (name.Length == 0)
				throw new ArgumentException("The display name for a WolTarget cannot be empty");
			Name = name ?? throw new ArgumentNullException("The display name for a WolTarget cannot be null");

			try
			{
				SetAddress(address);
				Port = port;
				ParseMac(mac);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		public WolTarget(WolTarget t)
		{
			//evaluate the name string, throw appropriate exception if necessary
			if (t.Name.Length == 0)
				throw new ArgumentException("The display name for a WolTarget cannot be empty");
			Name = t.Name ?? throw new ArgumentNullException("The display name for a WolTarget cannot be null");

			try
			{
				SetAddress(t.Address);
				Port = t.Port;
				ParseMac(t.Mac_string_array);
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		/// <summary>
		/// Sets the <see cref="Address"/> and <see cref="HostName"/> members
		/// </summary>
		/// <param name="address">An IPv4 address, IPv6 address, hostName, or url</param>
		/// <exception cref="ArgumentException">Thrown if an invalid parameter was passed, see the exception message for more info</exception>
		/// <exception cref="ArgumentNullException">Thrown if null was passed to a parameter, see the exception message for more info</exception>
		public void SetAddress(string address)
		{
			if (address == null)
				throw new ArgumentNullException("The display name for a WolTarget cannot be null");
			//evaluate the name string, throw appropriate exception if necessary
			if (address.Length == 0)
				throw new ArgumentException("The display name for a WolTarget cannot be empty");
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
		/// Sets the mac address
		/// </summary>
		/// <param name="mac">An array of strings representing the six bytes of the mac</param>
		/// <exception cref="ArgumentException">Thrown if the string did not conform to the correct format</exception>
		/// <exception cref="ArgumentNullException">Thrown if the string was null</exception>
		/// <exception cref="FormatException">Thrown if the string was not of the correct format.</exception>
		/// <exception cref="OverflowException">Thrown if a part of the string represents a number less than MinValue or greater than MaxValue or includes non-zero, fractional digits.</exception>
		public void SetMac(string[] mac)
		{
			try
			{
				ParseMac(mac);
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
			byte[] new_mac = new byte[6];
			//parse the mac's byte strings into a bytes in an array
			for (int i = 0; i < 6; i++)
			{
				//if anything was entered into this field, parse it
				if (macSubstrings[i] != "")
				{
					//if a single digit was entered into this field, pad with a zero
					if (macSubstrings[i].Length < 2)
						macSubstrings[i] = "0" + macSubstrings[i];
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
					macSubstrings[i] = "00";
					new_mac[i] = 0;
				}
			}
			//build new, padded, mac_string
			string new_Mac_string = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", macSubstrings[0], macSubstrings[1], macSubstrings[2], macSubstrings[3], macSubstrings[4], macSubstrings[5]);

			//now that everything parsed successfully, set the new values
			Mac_string = new_Mac_string;

			Mac = new_mac;
			Mac_string_array = macSubstrings;
		}
		/// <summary>
		/// Sets the port string on the object.
		/// </summary>
		/// <param name="port">The port to use</param>
		/// <exception cref="ArgumentNullException">Thrown if the string was null</exception>
		/// <exception cref="FormatException">Thrown if the string was not of the correct format.</exception>
		/// <exception cref="OverflowException">Thrown if a part of the string represents a number less than 0 or greater than 65535.</exception>
		/*public void SetPort(string port = "")
		{
			//default to port 9999 if an empty string was passed
			if (port == null)
				throw new ArgumentNullException("SetPort does not accept null as an input");
			if (port.Length == 0)
				Port = "";
			else
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
		}*/
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
		/// <returns>true if at least one packet was sent successfully, false if attempts failed</returns>
		public async Task<bool> SendMagicPacket()
		{
			int wakeAttemptCount = (int)AppData.GetSetting(AppData.Setting.wakeAttemptCount);
			bool result = false;
			for (int i = 0; i < wakeAttemptCount; i++)
			{
				if (AppData.debug)
				{
					Debug.WriteLine(string.Format("Sending packet {0} of {1}:", i, wakeAttemptCount));
					Debug.WriteLine(ToString());
				}

				DatagramSocket socket = new DatagramSocket();

				IOutputStream outStream;
				//get out stream to the selected target
				try
				{
					outStream = await socket.GetOutputStreamAsync(HostName, Port);
				}
				catch(Exception e){
					if (AppData.debug)
						Debug.WriteLine("An exception ocurred while creating the output stream:\n" + e.Message);
					else
						Debug.WriteLine("Sending failed");
					continue;
				}
				//obtain writer for the stream
				DataWriter writer = new DataWriter(outStream);
				//write the packet to the stream
				if (AppData.debug)
					Debug.WriteLine("Writing data...");
				writer.WriteBytes(MagicPacket());
				if (await writer.StoreAsync() == 102)
				{
					if (AppData.debug)
						Debug.WriteLine("Packet sent!");
					result = true;
				}
				else
				{
					if (AppData.debug)
						Debug.WriteLine("Sending the packet failed!");
				}
			}
			if (AppData.debug)
			{
				if (result)
					Debug.WriteLine("Succesfully sent the magic packet");
				else
					Debug.WriteLine("Failed to send the magic packet");
			}
			return result;
		}
		/// <summary>
		/// An override of the Object.ToString method.
		/// </summary>
		/// <returns>A strign representation of this host</returns>
		public override string ToString()
		{
			return string.Format("Display Name: {0}\nAddress: {1}\nMAC: {2}\nPort: {3}", Name, Address, Mac_string, Port);
		}
		public string AddressAndPortString()
		{
			return Address + ":" + Port;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			_ = reader.MoveToContent();
			Name = reader.GetAttribute("name");
			SetAddress(reader.GetAttribute("addr"));
			SetMac(reader.GetAttribute("mac"));
			Port = reader.GetAttribute("port");
			bool isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (!isEmptyElement)
			{
				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("addr", Address);
			writer.WriteAttributeString("mac", Mac_string);
			writer.WriteAttributeString("port", Port);
		}
		/// <summary>
		/// Determines whether the specified WolTarget object has an equivalent value to the current WolTarget object.
		/// </summary>
		/// <param name="obj">A WolTarget object that is compared with the current WolTarget.</param>
		/// <returns>A Boolean value that indicates whether the specified WolTarget object is equal to the current WolTarget object.</returns>
		public override bool Equals(object obj)
		{
			//catch null refs and type mismatches
			if ((obj == null) || !GetType().Equals(obj.GetType()))
				return false;
			WolTarget wolTarget = (WolTarget)obj;

			//test name
			if (!Name.Equals(wolTarget.Name))
				return false;
			//test address and hostname
			if (!(Address.Equals(wolTarget.Address) && HostName.IsEqual(wolTarget.HostName)))
				return false;
			//test mac, mac_string and mac_string_array
			if (!(Mac_string.Equals(wolTarget.Mac_string) && Enumerable.SequenceEqual(Mac_string_array, wolTarget.Mac_string_array) && Enumerable.SequenceEqual(Mac, wolTarget.Mac)))
				return false;
			//test port
			if (!Port.Equals(wolTarget.Port))
				return false;
			//if all were equal, return true
			return true;
		}
		/// <summary>
		/// Auto-generated GetHashCode method.
		/// </summary>
		/// <returns>A hash code for the current WolTarget object</returns>
		public override int GetHashCode()
		{
			int hashCode = -1060869375;
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Address);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Port);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Mac_string);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string[]>.Default.GetHashCode(Mac_string_array);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = (hashCode * -1521134295) + EqualityComparer<byte[]>.Default.GetHashCode(Mac);
			hashCode = (hashCode * -1521134295) + EqualityComparer<HostName>.Default.GetHashCode(HostName);
			return hashCode;
		}
	}

	public class AddressAndPortToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			WolTarget t = (WolTarget)value;
			return t.AddressAndPortString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
