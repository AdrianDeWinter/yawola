using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
		/// Indicates wether debug messages should be printed. Does not affect printing of exception messages
		/// </summary>
		public static readonly bool debug = true;

		public static readonly string logFileName = "log.txt";
		public static readonly string hostsFileName = "dataFile.txt";
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
				logFile = await localFolder.CreateFileAsync(logFileName, CreationCollisionOption.OpenIfExists);
				Debug.WriteLine("Had to create log file while storing state. Bug in startup logic?\n");
				await FileIO.AppendTextAsync(logFile, "Had to create log file while storing state. Bug in startup logic?\n");
			}
			logFile = await localFolder.CreateFileAsync(logFileName, CreationCollisionOption.OpenIfExists);

			await FileIO.AppendTextAsync(logFile, "Opening storage file\n");
			Debug.WriteLine("Opening storage file\n");
			StorageFile targetsFile = await localFolder.CreateFileAsync(hostsFileName, CreationCollisionOption.ReplaceExisting);
			Stream stream = await targetsFile.OpenStreamForWriteAsync();

			Debug.WriteLine("Writing to " + targetsFile.Path);
			await FileIO.AppendTextAsync(logFile, "Writing to " + targetsFile.Path);

			// Serialize the object, and close the TextWriter.
			serializer.Serialize(stream, targets);
			Debug.WriteLine("Stored " + targets.Count + " targets\n");
			await FileIO.AppendTextAsync(logFile, "Stored " + targets.Count + " targets\n");
		}
		/// <summary>
		/// Restores the application state from local storage
		/// </summary>
		/// <returns></returns>
		public static async Task LoadState()
		{
			logFile = await localFolder.CreateFileAsync(logFileName, CreationCollisionOption.ReplaceExisting);
			await FileIO.AppendTextAsync(logFile, "Begin reading targets file\n");
			StorageFile targetsFile;
			try
			{
				targetsFile = await localFolder.GetFileAsync(hostsFileName);
			}
			catch (Exception e)
			{
				Debug.WriteLine("Reading known hosts file failed: " + e.Message);
				await FileIO.AppendTextAsync(logFile, "Reading known hosts file failed: " + e.Message + "\n");
				return;
			}
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
