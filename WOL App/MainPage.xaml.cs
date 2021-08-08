using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;

namespace WOL_App
{
    public sealed partial class MainPage : Page
    {
        private readonly bool debug = true;

        private readonly DatagramSocket socket = new DatagramSocket();

        public readonly ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();

        public MainPage()
        {
            this.InitializeComponent();
            TargetList.ItemsSource = targets;
            if (debug)
                targets.Add(new WolTarget("192.168.188.128", "84:D8:1B:60:D6:AE", "mveServer", "9999"));
        }

        private async void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            WolTarget target = (WolTarget)TargetList.SelectedItem;
            if (debug)
            {
                Debug.WriteLine("Sending packet:");
                Debug.WriteLine(target.ToString());
            }

            //get out stream to the selected target
            IOutputStream outStream = null;
            try
            {
                outStream = await socket.GetOutputStreamAsync(target.HostName, target.Port);
            }
            catch(Exception except)
            {
                Debug.WriteLine("Send failed with error: " + except.Message);
                return;
            }
            //obtain writer for the stream
            DataWriter writer = new DataWriter(outStream);
            //write the packet to the stream
            if (debug)
                Debug.WriteLine("writing data");
            writer.WriteBytes(target.MagicPacket());
            //send the packet
            try
            {
                await writer.StoreAsync();
            }
            catch (Exception except)
            {
                Debug.WriteLine("Send failed with error: " + except.Message);
                return;
            }
            if (debug)
                Debug.WriteLine("Packet sent");
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            WolTarget target = new WolTarget(ipInput.Text, macInput.Text, clientNameInput.Text, "9999");
            targets.Add(target);
            if (debug)
            {
                Debug.WriteLine("Saved target:");
                Debug.WriteLine(target.HostName);
                Debug.WriteLine(target.Mac_string);
                Debug.WriteLine(target.Port);
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            targets.Remove((WolTarget)TargetList.SelectedItem);
        }
    }
}
