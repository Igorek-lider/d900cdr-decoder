﻿using D900Cdr.Decoder.Parselets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test.Decoder
{


    /// <summary>
    ///This is a test class for UssdStringParseletTest and is intended
    ///to contain all UssdStringParseletTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UssdStringParseletTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ValueAsString
        ///</summary>
        [TestMethod()]
        public void ValueAsStringTest()
        {
            byte[] value = new byte[] { 0xAA, 0x58, 0xCC, 0xA6, 0x9A, 0x8D, 0x1A };
            string expected = "*116*3#";
            string actual;
            actual = UssdStringParselet.ValueAsString(value);
            Assert.AreEqual(expected, actual);
        }
    }
}
