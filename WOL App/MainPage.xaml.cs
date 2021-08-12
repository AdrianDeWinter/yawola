using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace WOL_App
{
    public sealed partial class MainPage : Page
    {
        private readonly bool debug = true;

        private readonly TextBox[] popupFields = new TextBox[9];

        public readonly ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();

        public MainPage()
        {
            this.InitializeComponent();
            TargetList.ItemsSource = targets;
            popupFields[0] = clientNameInput;
            popupFields[1] = ipInput;
            popupFields[2] = macInput0;
            popupFields[3] = macInput1;
            popupFields[4] = macInput2;
            popupFields[5] = macInput3;
            popupFields[6] = macInput4;
            popupFields[7] = macInput5;
            popupFields[8] = portInput;
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            WolTarget target = (WolTarget)TargetList.SelectedItem;
            _ = target.SendMagicPacket(debug);
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
                Debug.WriteLine(target.Address);
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
            if (clientNameInput.Text.Length > 0 && ipInput.Text.Length > 0)
            {
                addHostDialog.IsPrimaryButtonEnabled = true;
                if (debug)
                    Debug.WriteLine("Validation successful");
                return;
            }
            addHostDialog.IsPrimaryButtonEnabled = false;
            if (debug)
                Debug.WriteLine("Validation failed");
            //wont check the mac any closer, correctness is suffieciently imposed by the input fields maxLength and Regex
            //ever field being blank will be parsed to 0x00:0x00:0x00:0x00:0x00:0x00, which is (theoretically) a valid mac
        }

        private void AddHostDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            foreach (TextBox box in popupFields)
                box.Text = "";
        }
    }
}
