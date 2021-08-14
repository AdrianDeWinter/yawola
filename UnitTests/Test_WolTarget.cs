using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WOL_App;

namespace UnitTests
{
    [TestClass]
    public class Test_WolTarget_Equals
    {
        [TestMethod]
        public void Test_Equals_null()
        {
            WolTarget t = new WolTarget("test", "1:2:3:4:5:6", "testhost", "1");
            Assert.AreNotEqual(t, null, "Equals returnes true when comapring a WolTarget object to a null value");
        }
        [TestMethod]
        public void Test_Equals_typeMismatch()
        {
            WolTarget t = new WolTarget("test", "1:2:3:4:5:6", "testhost", "1");
            Assert.AreNotEqual(t, new object(), "Equals returnes true when comapring a WolTarget object to an object fo another type");
        }
        [TestMethod]
        public void Test_Equals_NameDifferent()
        {
            WolTarget t1 = new WolTarget("test", "1:2:3:4:5:6", "testhost1", "1");
            WolTarget t2 = new WolTarget("test", "1:2:3:4:5:6", "testhost2", "1");
            Assert.AreNotEqual(t1, t2, "Equals returnes true on two WolTarget objects with different names");
        }
        [TestMethod]
        public void Test_Equals_AddressDifferent()
        {
            WolTarget t1 = new WolTarget("test1", "1:2:3:4:5:6", "testhost", "1");
            WolTarget t2 = new WolTarget("test2", "1:2:3:4:5:6", "testhost", "1");
            Assert.AreNotEqual(t1, t2, "Equals returnes true on two WolTarget objects with different addresses");
        }
        [TestMethod]
        public void Test_Equals_MacDifferent()
        {
            WolTarget t1 = new WolTarget("test", "1:2:3:4:5:a", "testhost", "1");
            WolTarget t2 = new WolTarget("test", "1:2:3:4:5:", "testhost", "1");
            Assert.AreNotEqual(t1, t2, "Equals returnes true on two WolTarget objects with different MACs");
        }
        [TestMethod]
        public void Test_Equals_PortDifferent()
        {
            WolTarget t1 = new WolTarget("test", "1:2:3:4:5:6", "testhost", "1");
            WolTarget t2 = new WolTarget("test", "1:2:3:4:5:6", "testhost", "2");
            Assert.AreNotEqual(t1, t2, "Equals returnes true on two WolTarget objects with different ports");
        }
        [TestMethod]
        public void Test_Equals_ActuallyEqual()
        {
            WolTarget t1 = new WolTarget("test", "1:2:3:4:5:6", "testhost", "1");
            WolTarget t2 = new WolTarget("test", "1:2:3:4:5:6", "testhost", "1");
            Assert.AreEqual(t1, t2, "Equals did not return true on two identical WolTarget objects");
        }
    }

    [TestClass]
    public class Test_WolTarget_Constructors
    {
        private static readonly string[] macElements = { "a1", "f5", "", "17", "a", "9" };
        private static readonly string[] macElementsPadded = { "a1", "f5", "00", "17", "0a", "09" };
        private static readonly string macString = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", macElements[0], macElements[1], macElements[2], macElements[3], macElements[4], macElements[5]);
        private static readonly string macStringPadded = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", macElementsPadded[0], macElementsPadded[1], macElementsPadded[2], macElementsPadded[3], macElementsPadded[4], macElementsPadded[5]);
        private static WolTarget tMacString;
        private static WolTarget tMacArray;

        [TestInitialize]
        public void TestInitialize()
        {
            tMacString = new WolTarget("addr", macString, "name", "1");
            tMacArray = new WolTarget("addr", macElements, "name", "1");
        }

        [TestMethod]
        public void Test_Constructor_MacString()
        {
            Assert.AreEqual(macStringPadded, tMacString.Mac_string, "Mac_string did not match expected value");
            CollectionAssert.AreEqual(macElementsPadded, tMacString.Mac_string_array, "Mac_string_array did not match expected value");
        }

        [TestMethod]
        public void Test_Constructor_MacStringArray()
        {
            Assert.AreEqual(macStringPadded, tMacArray.Mac_string, "Mac_string did not match expected value");
            CollectionAssert.AreEqual(macElements, tMacArray.Mac_string_array, "Mac_string_array did not match expected value");
        }

        [TestMethod]
        public void Test_Constructor_EqualResults()
        {
            Assert.AreEqual(tMacArray, tMacString, "The two constructors of WolTarget did not return equal results");
        }
    }
    [TestClass]
    public class Test_WolTarget_AddressAndPortString
    {
        private static WolTarget t;
        private static readonly string addr = "addr.de";
        private static readonly string port = "1";

        [TestInitialize]
        public void TestInitialize()
        {
            t = new WolTarget(addr, ":::::", "name", port);
        }

        [TestMethod]
        public void Test_AddressAndPortString()
        {
            Assert.AreEqual(addr + ":" + port, t.AddressAndPortString(), "AddressAndPortString returned an incorrect string");
        }
    }

    [TestClass]
    public class Test_WolTarget_SetPort
    {
        private static WolTarget t;

        [TestInitialize]
        public void TestInitialize()
        {
            t = new WolTarget("addr", ":::::", "name", "1");
        }

        [TestMethod]
        public void Test_SetPort()
        {
            string newPort = "2";
            t.SetPort(newPort);
            Assert.AreEqual(newPort, t.Port, "SetPort failed to set the new port information");
        }

        [TestMethod]
        public void Test_SetPort_EmptyString()
        {
            string newPort = "";
            t.SetPort(newPort);
            Assert.AreEqual("9999", t.Port, "SetPort did not default to port 9999 when given an empty string");
        }

        [TestMethod]
        public void Test_SetPort_NoString()
        {
            t.SetPort();
            Assert.AreEqual("9999", t.Port, "SetPort did not default to port 9999 when given no string");
        }

        [TestMethod]
        public void Test_SetPort_NullString()
        {
            _ = Assert.ThrowsException<ArgumentNullException>(delegate () { t.SetPort(null); }, "SetPort did not throw an ArgumentNullException when given a null value");
        }

        [TestMethod]
        public void Test_SetPort_ValueOverflow()
        {
            _ = Assert.ThrowsException<OverflowException>(delegate () { t.SetPort(int.MaxValue.ToString()); }, "SetPort did not throw an OverflowException when given a value greater than the max possible port number");
        }

        [TestMethod]
        public void Test_SetPort_IncorrectFormat_decimal()
        {
            _ = Assert.ThrowsException<FormatException>(delegate () { t.SetPort("1.2"); }, "SetPort did not throw a FormatException when given a decimal value");
        }

        [TestMethod]
        public void Test_SetPort_IncorrectFormat_letters()
        {
            _ = Assert.ThrowsException<FormatException>(delegate () { t.SetPort("1f"); }, "SetPort did not throw a FormatException when given a string containing lettters");
        }
    }
}
