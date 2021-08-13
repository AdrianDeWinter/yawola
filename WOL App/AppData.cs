using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace WOL_App
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
		/// Indicates wether debug messages should be printed. Does not affect printing of exception messages
		/// </summary>
		public static readonly bool debug = true;

		/// <summary>
		/// reference to the app instamces local storage folder
		/// </summary>
		private static readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
		/// <summary>
		/// An XML Serializer for <see cref="System.Collections.ObjectModel.ObservableCollection"/>'s of <see cref="WolTarget"/>
		/// </summary>
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<WolTarget>));
		/// <summary>
		/// The log.txt file in local storage used for logging purposes
		/// </summary>
		private static StorageFile logFile;
		/// <summary>
		/// Saves the current application state to local storage
		/// </summary>
		/// <returns></returns>
		public static async Task SaveState()
		{
			if (logFile == null)
			{
				logFile = await localFolder.CreateFileAsync("log.txt", CreationCollisionOption.OpenIfExists);
				await FileIO.AppendTextAsync(logFile, "Had to create log file while storing state. Bug in startup logic?\n");
			}
			await FileIO.AppendTextAsync(logFile, "Opening storage file\n");
			StorageFile targetsFile = await localFolder.CreateFileAsync("dataFile.txt", CreationCollisionOption.ReplaceExisting);
			Stream stream = await targetsFile.OpenStreamForWriteAsync();
			Debug.WriteLine("Writing to " + targetsFile.Path);
			// Serialize the object, and close the TextWriter.
			serializer.Serialize(stream, targets);
			await FileIO.AppendTextAsync(logFile, "Stored " + targets.Count + " targets\n");
		}
		/// <summary>
		/// Restores the application state from local storage
		/// </summary>
		/// <returns></returns>
		public static async Task LoadState()
		{
			logFile = await localFolder.CreateFileAsync("log.txt", CreationCollisionOption.ReplaceExisting);
			await FileIO.AppendTextAsync(logFile,"Begin reading targets file\n");
			StorageFile targetsFile = await localFolder.GetFileAsync("dataFile.txt");
			Stream reader = await targetsFile.OpenStreamForReadAsync();
			Debug.WriteLine("Reading from " + targetsFile.Path);
			try
			{
				foreach (WolTarget t in (ObservableCollection<WolTarget>)serializer.Deserialize(reader))
					targets.Add(t);
			}
			catch (Exception e){
				Debug.WriteLine(e.Message);
				await FileIO.AppendTextAsync(logFile, e.Message + "\n");
			}
			await FileIO.AppendTextAsync(logFile, "Restored " + targets.Count + " targets\n");
		}
	}
}
