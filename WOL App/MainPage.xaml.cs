using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;

namespace WOL_App
{
    public sealed partial class MainPage : Page
    {
        private readonly bool debug = true;

        public readonly ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();

        public MainPage()
        {
            this.InitializeComponent();
            TargetList.ItemsSource = targets;
            if (debug)
                targets.Add(new WolTarget("192.168.188.128", "84:D8:1B:60:D6:AE", "mveServer", "9999"));
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            WolTarget target = (WolTarget)TargetList.SelectedItem;
            target.SendMagicPacket(debug);
        }

        private void Add_Host(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string[] mac = {
                macInput0.Text, macInput1.Text ,
                macInput2.Text, macInput3.Text ,
                macInput4.Text, macInput5.Text
            };
            WolTarget target = new WolTarget(ipInput.Text, mac, clientNameInput.Text, portInput.Text);
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
            _ = targets.Remove((WolTarget)TargetList.SelectedItem);
        }

        private async void Open_Add_Dialog(object sender, RoutedEventArgs e)
        {
            _ = await addHostDialog.ShowAsync();
        }

        private void ValidateAddHostForm(object sender, TextChangedEventArgs args)
        {
            if (debug)
                Debug.WriteLine("Validating Form...");
            //host/ip and display name must be set
            if (clientNameInput.Text != "" && ipInput.Text != "")
                addHostDialog.IsPrimaryButtonEnabled = true;
            addHostDialog.IsPrimaryButtonEnabled = false;
            //wont check the mac any closer, correctness is suffieciently imposed by the input fields maxLength and Regex
            //ever field being blank will be parsed to 0x00:0x00:0x00:0x00:0x00:0x00, which is (theoretically) a valid mac
        }
    }
}
