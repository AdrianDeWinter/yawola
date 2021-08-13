using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace WOL_App
{
	public static class AppData
	{
		public static readonly ObservableCollection<WolTarget> targets = new ObservableCollection<WolTarget>();
		private static readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<WolTarget>));
		private static StorageFile logFile;
		public static async Task SaveState()
		{
			/*if (logFile == null)
			{
				logFile = await localFolder.CreateFileAsync("log.txt", CreationCollisionOption.ReplaceExisting);
				await FileIO.AppendTextAsync(logFile, "Had to create log file while storing state. Bug in startup logic?\n");
			}*/
			await FileIO.AppendTextAsync(logFile, "Opening storage file\n");
			StorageFile targetsFile = await localFolder.CreateFileAsync("dataFile.txt", CreationCollisionOption.ReplaceExisting);
			Stream stream = await targetsFile.OpenStreamForWriteAsync();
			Debug.WriteLine("Writing to " + targetsFile.Path);
			// Serialize the object, and close the TextWriter.
			serializer.Serialize(stream, targets);
			await FileIO.AppendTextAsync(logFile, "Stored " + targets.Count + " targets\n");
		}
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
