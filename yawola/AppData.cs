using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace yawola
{
	/// <summary>
	/// Holds any data that should be shared beween classes and/or persistet across application launches. Also handles storing and restoring data.
	/// </summary>
	public static class AppData
	{
		/// <summary>
		/// The <see cref="WolTarget"/>'s displayed in the <see cref="MainPage"/>'s <see cref="MainPage.TargetList"/>
		/// </summary>
		public static readonly ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();
		/// <summary>
		/// Contains all settings
		/// </summary>
		private static readonly Dictionary<Setting, object> settings = new Dictionary<Setting, object>();
		/// <summary>
		/// The default settings
		/// </summary>
		private static readonly Dictionary<Setting, object> defaultSettings = new Dictionary<Setting, object>()
		{
			{Setting.windowWidth, 490 },
			{Setting.windowHeight, 350 },
			{Setting.useRoamingSettings, true },
			{Setting.wakeAttemptCount, 3 },
			{Setting.defaultPort, 9999 }
		};
		/// <summary>
		/// This Enum represents each possible setting
		/// </summary>
		public enum Setting {
			none,
			windowWidth,
			windowHeight,
			useRoamingSettings,
			wakeAttemptCount,
			defaultPort
		}
		/// <summary>
		/// Indicates wether debug messages should be printed. Does not affect printing of exception messages
		/// </summary>
		public static readonly bool debug = true;
		/// <summary>
		/// file name used to save the log file for the current app run
		/// </summary>
		public static readonly string logFileName = "log.txt";
		/// <summary>
		/// file name used to store serialized app data
		/// </summary>
		public static readonly string hostsFileName = "dataFile.txt";
		/// <summary>
		/// reference to the app instamces local storage folder
		/// </summary>
		private static StorageFolder dataFolder;
		/// <summary>
		/// An XML Serializer for <see cref="System.Collections.ObjectModel.ObservableCollection"/>'s of <see cref="WolTarget"/>
		/// </summary>
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<WolTarget>));
		/// <summary>
		/// The log.txt file in local storage used for logging purposes
		/// </summary>
		private static StorageFile logFile;
		/// <summary>
		/// The current settings container
		/// </summary>
		private static ApplicationDataContainer settingsContainer;
		/// <summary>
		/// Saves the current application state to local storage
		/// </summary>
		/// <returns></returns>
		public static async Task SaveState()
		{
			if (logFile == null)
			{
				logFile = await dataFolder.CreateFileAsync(logFileName, CreationCollisionOption.OpenIfExists);
				Debug.WriteLine("Had to create log file while storing state. Bug in startup logic?\n");
				await FileIO.AppendTextAsync(logFile, "Had to create log file while storing state. Bug in startup logic?\n");
			}
			logFile = await dataFolder.CreateFileAsync(logFileName, CreationCollisionOption.OpenIfExists);
			await SaveSettings();
			await SaveTargets();
		}

		/// <summary>
		/// Restores the application state from local storage
		/// </summary>
		/// <returns></returns>
		public static async Task LoadState()
		{
			//make sure the settings and datafiolder references are set correctly
			GetSettingsContainerAndDataFolder();

			//create a new logfile
			logFile = await dataFolder.CreateFileAsync(logFileName, CreationCollisionOption.ReplaceExisting);
			await LoadSettings();
			await LoadTargets();

			//register event handler for any updates to the roaming data after application launch
			ApplicationData.Current.DataChanged += DataChangedEventHandler;
		}
		/// <summary>
		/// Reacts to the <see cref="ApplicationData.DataChanged"/> event by reloading the settings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private static async void DataChangedEventHandler(ApplicationData sender, object args)
		{
			if ((bool)GetSetting(Setting.useRoamingSettings))
			{
				await LoadSettings();
				await LoadTargets();
			}
		}
		/// <summary>
		/// Reads and deserializes the list of targets from <see cref="dataFolder"/>/<see cref="hostsFileName"/>
		/// </summary>
		private static async Task LoadTargets()
		{
			//attempt to get the targets list
			await FileIO.AppendTextAsync(logFile, "Begin reading targets file...\n");
			StorageFile targetsFile;
			try
			{
				targetsFile = await dataFolder.GetFileAsync(hostsFileName);
			}
			catch (Exception e)
			{
				Debug.WriteLine("Reading known hosts file failed: " + e.Message);
				await FileIO.AppendTextAsync(logFile, "Reading known hosts file failed: " + e.Message + "\n");
				return;
			}

			//attept to read and deserialize the targets list
			Stream reader = await targetsFile.OpenStreamForReadAsync();
			Debug.WriteLine("Reading from " + targetsFile.Path);
			ObservableCollection<WolTarget> temp = new ObservableCollection<WolTarget>();
			try
			{
				foreach (WolTarget t in (ObservableCollection<WolTarget>)serializer.Deserialize(reader))
					temp.Add(t);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				await FileIO.AppendTextAsync(logFile, e.Message + "\n");
			}
			//if all WolTargets parsed successfully, copy them into the actual collection
			targets.Clear();
			foreach (WolTarget t in temp)
				targets.Add(t);
			Debug.WriteLine("Restored " + targets.Count + " target(s)");
			await FileIO.AppendTextAsync(logFile, "Restored " + targets.Count + " target(s)\n");
		}
		/// <summary>
		/// Reads the <see cref="settingsContainer"/> and uses it to fill <see cref="settings"/>.
		/// </summary>
		/// <remarks>
		/// Iterates over <see cref="defaultSettings"/> and checks if an entry with that key exists in <see cref="settingsContainer"/>.
		/// If so, that value is added to <see cref="settings"/>. Otherwise, the default value is used.
		/// </remarks>
		private static async Task LoadSettings()
		{
			await FileIO.AppendTextAsync(logFile, "Reading settings file...\n");
			settings.Clear();
			foreach (KeyValuePair<Setting, object> s in defaultSettings)
			{
				if (settingsContainer.Values.TryGetValue(((int)s.Key).ToString(), out object o))
					settings.Add(s.Key, o);
				else
					settings.Add(s.Key, s.Value);
			}
			settings.Add(Setting.none, "");
			await FileIO.AppendTextAsync(logFile, "Finished reading settings file\n");
		}
		/// <summary>
		/// Saves the settings in <see cref="settings"/> to <see cref="settingsContainer"/>
		/// </summary>
		/// <remarks>
		/// Iterates over <see cref="settings"/> and writes each key/value pair to <see cref="settingsContainer"/>
		/// </remarks>
		private static async Task SaveSettings()
		{
			await FileIO.AppendTextAsync(logFile, "Writing settings to file...\n");
			foreach (KeyValuePair<Setting, object> s in settings)
			{
				string key = ((int)s.Key).ToString();
				if (settingsContainer.Values.ContainsKey(key))
					_ = settingsContainer.Values.Remove(key);
				settingsContainer.Values.Add(key, s.Value);
			}
			await FileIO.AppendTextAsync(logFile, "Finished writing settings to file\n");
		}
		/// <summary>
		/// Updates a key/value pair in <see cref="settings"/> with a new value
		/// </summary>
		/// <param name="key">The key to update</param>
		/// <param name="value">The new value</param>
		/// <exception cref="ArgumentException">Thrown if the key does not exist in the settings, or could not be romved</exception>
		public static void UpdateSetting(Setting key, object value){
			if (!settings.Remove(key))
				throw new ArgumentException("The key \"" + key + "\" does not exist in the settings");
			settings.Add(key, value);
		}
		/// <summary>
		/// Updates a key/value pair in <see cref="settings"/> with a new value
		/// </summary>
		/// <param name="key">The key to update</param>
		/// <param name="value">The new value</param>
		/// <exception cref="ArgumentException">Thrown if the key does not exist in the settings</exception>
		public static object GetSetting(Setting key)
		{
			if (!settings.TryGetValue(key, out object o))
				throw new ArgumentException("The key \"" + key + "\" does not exist in the settings");
			return o;
		}
		/// <summary>
		/// Serializes the list of <see cref="WolTarget"/>s in <see cref="targets"/> to <see cref="dataFolder"/>/<see cref="hostsFileName"/>
		/// </summary>
		private static async Task SaveTargets()
		{
			await FileIO.AppendTextAsync(logFile, "Opening storage file\n");
			Debug.WriteLine("Opening storage file");
			//create and open targets file
			StorageFile targetsFile = await dataFolder.CreateFileAsync(hostsFileName, CreationCollisionOption.ReplaceExisting);
			Stream stream = await targetsFile.OpenStreamForWriteAsync();

			Debug.WriteLine("Writing to " + targetsFile.Path);
			await FileIO.AppendTextAsync(logFile, "Writing to " + targetsFile.Path + "\n");

			// Serialize the objects, and close the TextWriter.
			serializer.Serialize(stream, targets);
			Debug.WriteLine("Stored " + targets.Count + " targets");
			await FileIO.AppendTextAsync(logFile, "Stored " + targets.Count + " targets\n");
		}
		/// <summary>
		/// Retrieves or creates the local or roaming settings container and data folder
		/// </summary>
		public static void GetSettingsContainerAndDataFolder()
		{
			//try to retrieve the local settings container. if it is not initialized, retrieve the roaming one
			settingsContainer = ApplicationData.Current.LocalSettings.Values.ContainsKey(((int)Setting.none).ToString())
				? ApplicationData.Current.LocalSettings
				: ApplicationData.Current.RoamingSettings;
			settings.Clear();
			//write the determined locality into settings
			settings.Add(Setting.useRoamingSettings, settingsContainer.Locality == ApplicationDataLocality.Roaming);

			//get the appropriate data folder
			dataFolder = (bool)GetSetting(Setting.useRoamingSettings)
				? ApplicationData.Current.RoamingFolder
				: ApplicationData.Current.LocalFolder;
		}

		/// <summary>
		/// Switches between roaming or local settings and moves the settings and data files appropriately
		/// </summary>
		public static async void SwitchLocalOrRoamingData()
		{
			ApplicationDataLocality currentLocality = settingsContainer.Locality;
			if (currentLocality == ApplicationDataLocality.Local)
				Debug.WriteLine("currently on local, switching to roaming...");
			else
				Debug.WriteLine("currently on roaming, switching to local...");
			//get the new settings container of opposite locality
			ApplicationDataContainer newSettings = currentLocality == ApplicationDataLocality.Local
				? ApplicationData.Current.RoamingSettings
				: ApplicationData.Current.LocalSettings;

			//get the new data folder of opposite locality
			StorageFolder newFolder = currentLocality == ApplicationDataLocality.Local
			? ApplicationData.Current.RoamingFolder
			: ApplicationData.Current.LocalFolder;

			//get an array of the key/value pairs currently saved in the settings
			KeyValuePair<string, object>[] settingsPairs = new KeyValuePair<string, object>[settingsContainer.Values.Count];
			settingsContainer.Values.CopyTo(settingsPairs, 0);

			//ensure the new container is empty
			newSettings.Values.Clear();

			//copy all settings into the new container
			foreach (KeyValuePair<string, object> p in settingsPairs)
				newSettings.Values.Add(p);

			//ensure the target folder is empty
			foreach (IStorageItem f in await newFolder.GetItemsAsync())
				await f.DeleteAsync();

			//copy all files from the old folder to the new one
			foreach (StorageFile f in await dataFolder.GetFilesAsync())
				await f.MoveAsync(newFolder);

			//clean ot the old locations
			settingsContainer.Values.Clear();
			foreach (IStorageItem f in await dataFolder.GetItemsAsync())
				await f.DeleteAsync();

			//set references to the new locations
			settingsContainer = newSettings;
			dataFolder = newFolder;
		}
	}
}
