using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Uwp.UI;

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
            WolTarget target = new WolTarget(ipInput.Text, macInput1.Text, clientNameInput.Text, "9999");
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

        private void MacInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox activeBox = (TextBox)sender;
            if (!TextBoxExtensions.GetIsValid(activeBox))
            {

            }
            switch (activeBox.Name)
            {
                case "macInput1": break;
                case "macInput2": break;
                case "macInput3": break;
                case "macInput4": break;
                case "macInput5": break;
                case "macInput6": break;
                default:
                    break;
            }
        }

        private async void Open_Add_Dialog(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await addHostDialog.ShowAsync();
        }
    }
}
