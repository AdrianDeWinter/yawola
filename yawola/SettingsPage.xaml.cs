using System;
using System.Collections.Generic;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace yawola
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			InitializeComponent();
			RoamSettingsSwitch.IsOn = (bool)AppData.GetSetting(AppData.Setting.useRoamingSettings);
			WakeAttemptInput.Text = ((int)AppData.GetSetting(AppData.Setting.wakeAttemptCount)).ToString();
			DefaultPortInput.Text = ((int)AppData.GetSetting(AppData.Setting.defaultPort)).ToString();
			//set event handlers after ui elements are prefilled to avoid firing events unnecessarily
			RoamSettingsSwitch.Toggled += RoamSettingsSwitch_Toggled;
			WakeAttemptInput.TextChanged += WakeAttemptInput_TextChanged;
			DefaultPortInput.TextChanged += DefaultPortInput_TextChanged;
		}

		private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			_ = Frame.Navigate(typeof(MainPage));
		}

		private void RoamSettingsSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			AppData.SwitchLocalOrRoamingData();
			AppData.UpdateSetting(AppData.Setting.useRoamingSettings, ((ToggleSwitch)sender).IsOn);
		}

		private void WakeAttemptInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			AppData.UpdateSetting(AppData.Setting.wakeAttemptCount, int.Parse(((TextBox)sender).Text));
		}

		private void DefaultPortInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			AppData.UpdateSetting(AppData.Setting.defaultPort, int.Parse(((TextBox)sender).Text));
		}
	}
}
