using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Storage;
using WOL_App;
namespace UnitTests
{
    [TestClass]
    public class Test_AppData
    {
        private static WolTarget t;
        private static readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<WolTarget>));
        private static StorageFile testFile;
        [TestInitialize]
        public async Task TestInitialize()
        {
            //prepare test data
            t = new WolTarget("192.168.1.2", "1a:Af::1:a:B", "testhost", "1234");
            AppData.targets.Clear();

            //clear the localstorage folder
            IReadOnlyList<StorageFile> files = await localFolder.GetFilesAsync();
            foreach (StorageFile f in files)
                await f.DeleteAsync(StorageDeleteOption.PermanentDelete);

            //prepare testdata file
            testFile = await localFolder.CreateFileAsync("testdata", CreationCollisionOption.ReplaceExisting);
            Stream stream = await testFile.OpenStreamForWriteAsync();
            serializer.Serialize(stream, new ObservableCollection<WolTarget> { t });
        }


        /// <summary>
        /// Test a simple case of storing something into <see cref="AppData"/> and then serializing and deserializing it.
        /// Specifically, tests <see cref="AppData.SaveState"/> and <see cref="AppData.LoadState"/> in that order.
        /// </summary>
        [TestMethod]
        public async Task Test_SaveLoad()
        {
            AppData.targets.Add(t);
            Assert.AreEqual(t, AppData.targets[0], "WolTarget object was not correctly added to the AppData.targets collection");

            //serialize, clear and deserialize
            await AppData.SaveState();
            AppData.targets.Clear();
            _ = Assert.ThrowsException<ArgumentOutOfRangeException>(delegate () { _ = AppData.targets[0]; }, "The AppData.targets collection was not cleared correctly");
            
            await AppData.LoadState();

            //assert that the restored data matches the originial data
            Assert.AreEqual(t, AppData.targets[0], "The restored WolTarget object was not equal to the original");
        }

        [TestMethod]
        public async Task Test_Save_CleanFolder()
        {
            AppData.targets.Add(t);
            await AppData.SaveState();
            try
            {
                _ = await localFolder.GetFileAsync(AppData.hostsFileName);
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("AppData.SaveState() did not properly generate {0}: {1}", AppData.hostsFileName, ex.Message));
            }
        }

        [TestMethod]
        public async Task Test_Save_ExistingFile()
        {
            _ = await testFile.CopyAsync(localFolder, AppData.hostsFileName);
            AppData.targets.Add(t);
            await AppData.SaveState();
            try
            {
                _ = await localFolder.GetFileAsync(AppData.hostsFileName);
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("AppData.SaveState() did not properly generate {0}: {1}", AppData.hostsFileName, ex.Message));
            }
        }
        
        [TestMethod]
        public async Task Test_Load()
        {
            _ = await testFile.CopyAsync(localFolder, AppData.hostsFileName);
            try
            {
                await AppData.LoadState();
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("AppData.LoadState() did not properly load {0}: {1}", AppData.hostsFileName, ex.Message));
            }
            Assert.AreEqual(1, AppData.targets.Count, "AppData.LoadState() did not restore the correct number of hosts");
            Assert.AreEqual(t, AppData.targets[0], "AppData.LoadState() did not restore the host correctly");
        }

        [TestMethod]
        public async Task Test_Load_EmptyFile()
        {
            _ = await localFolder.CreateFileAsync(AppData.hostsFileName);
            try
            {
                await AppData.LoadState();
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("AppData.LoadState() did not properly handle an empty {0}: {1}", AppData.hostsFileName, ex.Message));
            }
        }

        [TestMethod]
        public async Task Test_Load_NoFile()
        {
            try
            {
                await AppData.LoadState();
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("AppData.LoadState() did not properly handle a non existing {0}: {1}", AppData.hostsFileName, ex.Message));
            }
        }
    }
}
