using D900Cdr.Decoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Test.Decoder
{
    
    
    /// <summary>
    ///This is a test class for CdrDecoderTest and is intended
    ///to contain all CdrDecoderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CdrDecoderTest
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
        ///A test for CheckElementsDefinition
        ///</summary>
        [DeploymentItem("C:\\Projects\\D900Cdr.Decoder\\SampleCDRs\\SR13\\KLG_02Dec2008_181815.cdr"), DeploymentItem("D:\\Projects\\D900Cdr.Decoder\\D900Cdr.Decoder\\D900Cdr.Elements.xml"), TestMethod()]
        public void CheckElementsDefinitionTest()
        {
            CdrDecoder target = new CdrDecoder();
            target.ElementDefinitionProvider.CurrentSchema = "CS-50";
            Stream asnStream = new FileStream("KLG_02Dec2008_181815.cdr", FileMode.Open);
            TextWriter dumpWriter = new StreamWriter("CheckElementsDefinition.log");
            int expected = 2;
            int actual;
            actual = target.CheckElementsDefinition(asnStream, dumpWriter);
            Assert.AreEqual(expected, actual);

            asnStream.Close();
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CheckElementsDefinition (Batch)
        ///</summary>
        [DeploymentItem("D:\\Projects\\D900Cdr.Decoder\\D900Cdr.Decoder\\D900Cdr.Elements.xml"), TestMethod()]
        public void CheckElementsDefinitionBatchTest()
        {
            CdrDecoder target = new CdrDecoder(); 
            target.ElementDefinitionProvider.CurrentSchema = "CS-50";
            String srcPath = @"C:\Projects\D900Cdr.Decoder\SampleCDRs\SR13";
            TextWriter dumpWriter = new StreamWriter("CheckElementsDefinition.log");
            int expected = 2;
            int actual;

            FileInfo[] cdrFiles;

            cdrFiles = new DirectoryInfo(srcPath).GetFiles("*.cdr", SearchOption.TopDirectoryOnly);
            FileStream cdr;

            foreach (FileInfo fi in cdrFiles)
            {
                cdr = new FileStream(fi.FullName, FileMode.Open);

                actual = target.CheckElementsDefinition(cdr, dumpWriter);
                Assert.AreEqual(expected, actual);

                cdr.Close();
            }
        }

        /// <summary>
        ///A test for Dump
        ///</summary>
        [DeploymentItem(@"C:\users\adn\Bercut\sml_cdr\mcrsm_6504_089521.mcr"), DeploymentItem("D:\\Projects\\D900Cdr.Decoder\\D900Cdr.Decoder\\D900Cdr.Elements.xml"), TestMethod()]
        public void DumpAllTest()
        {
            CdrDecoder target = new CdrDecoder();
            target.ElementDefinitionProvider.CurrentSchema = "CS-50";
            Stream asnStream = new FileStream(@"mcrsm_6504_089521.mcr", FileMode.Open);
            byte maxLevel = byte.MaxValue;
            int expected = 2;
            int actual;

            TextWriter dumpWriter1 = new StreamWriter("Dump_ELEMENT_TXT.log");
            actual = target.Dump(asnStream, dumpWriter1, DumpType.ELEMENT_TXT, maxLevel);
            Assert.AreEqual(expected, actual);

            asnStream.Seek(0, SeekOrigin.Begin);
            TextWriter dumpWriter2 = new StreamWriter("Dump_ELEMENT_XML.log");
            actual = target.Dump(asnStream, dumpWriter2, DumpType.ELEMENT_XML, maxLevel);
            Assert.AreEqual(expected, actual);

            asnStream.Seek(0, SeekOrigin.Begin);
            TextWriter dumpWriter3 = new StreamWriter("Dump_TLV_TXT.log");
            actual = target.Dump(asnStream, dumpWriter3, DumpType.TLV_TXT, maxLevel);
            Assert.AreEqual(expected, actual);

            asnStream.Seek(0, SeekOrigin.Begin);
            TextWriter dumpWriter4 = new StreamWriter("Dump_TLV_XML.log");
            actual = target.Dump(asnStream, dumpWriter4, DumpType.TLV_XML, maxLevel);
            Assert.AreEqual(expected, actual);

            asnStream.Close();
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
