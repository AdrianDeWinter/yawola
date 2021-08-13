using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace WOL_App
{
	public sealed partial class MainPage : Page
	{
		/// <summary>
		/// Reference to the apps localStorage
		/// </summary>
		StorageFolder localFolder = ApplicationData.Current.LocalFolder;

		/// <summary>
		/// Indicates wether debug messages should be printed. Does not affect printing of exception messages
		/// </summary>
		private readonly bool debug = true;

		/// <summary>
		/// an array holding references to all fields in the <see cref="addHostDialog"/> to enable easier removal of inputted text wehn the user cancels the operation
		/// </summary>
		private readonly TextBox[] popupFields = new TextBox[9];

		/// <summary>
		/// the list of targets dispalyed in <see cref="TargetList"/>
		/// </summary>
		public ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();

		/// <summary>
		/// The constructor called by the system when MainPage is to be displayed. Initializes the page and performs any preperation necessary
		/// </summary>
		public MainPage()
		{
			this.InitializeComponent();
			//set the listViews ItemsSource. Has to be done here because it can only be performed after both the ListView and and OberservableCollection have been initialized
			//TargetList.ItemsSource = targets;
			TargetList.ItemsSource = AppData.targets;
			//fill the popupFields array with references to all input fields in the addHostDialog
			popupFields[0] = clientNameInput;
			popupFields[1] = ipInput;
			popupFields[2] = macInput0;
			popupFields[3] = macInput1;
			popupFields[4] = macInput2;
			popupFields[5] = macInput3;
			popupFields[6] = macInput4;
			popupFields[7] = macInput5;
			popupFields[8] = portInput;

			//this.testXml();
		}

		/// <summary>
		/// Callbck for the "Send" button, retrieves the selected item from <see cref="TargetList"/> and orders a magic packet to be sent 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Send_Button_Click(object sender, RoutedEventArgs e)
		{
			WolTarget target = (WolTarget)TargetList.SelectedItem;
			_ = target.SendMagicPacket(debug);
		}

		/// <summary>
		/// Creates a new instance of WolTarget and adds it to <see cref="targets"/>
		/// </summary>
		private void Add_Host()
		{
			string[] mac = {
				macInput0.Text, macInput1.Text ,
				macInput2.Text, macInput3.Text ,
				macInput4.Text, macInput5.Text
			};
			WolTarget target = new WolTarget(ipInput.Text, mac, clientNameInput.Text, portInput.Text);
			AppData.targets.Add(target);
			if (debug)
			{
				Debug.WriteLine("Saved target:");
				Debug.WriteLine(target.Address);
				Debug.WriteLine(target.Mac_string);
				Debug.WriteLine(target.Port);
			}
		}

		/// <summary>
		/// Callback for the "Delete" button. Removes the selectged entry from <see cref="targets"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Delete_Button_Click(object sender, RoutedEventArgs e)
		{
			_ = AppData.targets.Remove((WolTarget)TargetList.SelectedItem);
		}

		/// <summary>
		/// Callback for the "Add" button. Displays <see cref="addHostDialog"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Open_Add_Dialog(object sender, RoutedEventArgs e)
		{
			_ = await addHostDialog.ShowAsync();
		}

		/// <summary>
		/// Validates the inputs in <see cref="addHostDialog"/> and enbles its primaryButton if the inputs are valid.<br/>
		/// The form is considered valid if the client name and address are not empty
		/// </summary>
		/// <remarks>
		/// The mac is not checked, since its correctness is suffieciently imposed by the input fields maxLength and Regex.<br/>
		/// Every field being blank will be parsed to 0x00:0x00:0x00:0x00:0x00:0x00, which is (theoretically) a valid mac.<br/>
		/// The same goes for the port (which just defaults to "0")</remarks>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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
		}
		/// <summary>
		/// Function to clear all fields in the addHostDialog. Is called by <see cref="Add_Host"/> and <see cref="AddHostDialog_CloseButtonClick"/>
		/// </summary>
		private void ClearDialogFields()
		{
			foreach (TextBox box in popupFields)
				box.Text = "";
		}
		/// <summary>
		/// Callback for the <see cref="addHostDialog"/>'s primary button. Clears all inputs made in the dialog and adds the new host to <see cref="targets"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void AddHostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Add_Host();
			ClearDialogFields();
		}

		/// <summary>
		/// Callback for the <see cref="addHostDialog"/>'s secondary button. Clears all inputs made in the dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void AddHostDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			ClearDialogFields();
		}

		/// <summary>
		/// Event handler for the SelectionChanged event on the <see cref="TargetList"/>. Updates the <see cref="SendButton"/>'s IsEnabled property
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TargetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SendButton.IsEnabled = TargetList.SelectedItem != null;
		}

		private async void testXml()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<WolTarget>));
			ObservableCollection<WolTarget> temp = new ObservableCollection<WolTarget>
			{
				new WolTarget("bla", "0:0:0:0:0:0", "blub", "1")
			};
			StorageFile sampleFile = await localFolder.CreateFileAsync("dataFile.txt", CreationCollisionOption.ReplaceExisting);
			Stream stream = await sampleFile.OpenStreamForWriteAsync();
			// Serialize the object, and close the TextWriter.
			serializer.Serialize(stream, temp);
			stream.Close();
			Stream reader = await sampleFile.OpenStreamForReadAsync();
			foreach (WolTarget t in (ObservableCollection<WolTarget>)serializer.Deserialize(reader))
				targets.Add(t);
		}
	}
}
