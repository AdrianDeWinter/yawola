using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WOL_App;
namespace UnitTests
{
    [TestClass]
    public class Test_AppData
    {
        /// <summary>
        /// Test a simple case of storing something into <see cref="AppData"/> and then serializing and deserializing it.
        /// Specifically, tests <see cref="AppData.SaveState"/> and <see cref="AppData.LoadState"/> in that order.
        /// </summary>
        [TestMethod]
        public async Task Test_SaveLoad()
        {
            //prepare test data
            WolTarget t = new WolTarget("192.168.1.2", "1a:Af::1:a:B", "testhost", "1234");
            AppData.targets.Add(t);
            Assert.AreEqual(t, AppData.targets[0], "WolTarget object was not correctly added to the AppData.targets collection");

            //serialize, clear and deserialize
            await AppData.SaveState();
            AppData.targets.Clear();
            Assert.ThrowsException<ArgumentOutOfRangeException>(delegate () { WolTarget _ = AppData.targets[0]; }, "The AppData.targets collection was not cleared correctly");
            
            await AppData.LoadState();

            //assert that the restored data matches the originial data
            Assert.AreEqual(t, AppData.targets[0], "The restored WolTarget object was not equal to the original");
        }
    }
}
