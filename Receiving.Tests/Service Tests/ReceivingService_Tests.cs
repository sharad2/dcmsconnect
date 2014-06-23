using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.Receiving.Repository;
using Moq;
using System.Web;
using DcmsMobile.Receiving.Models;
using System.Configuration.Provider;

namespace Receiving.Tests.Service_Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ReceivingService_Tests
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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        ///// <summary>
        ///// Must return true for pallets eginning with P
        ///// </summary>
        //[TestMethod]
        //public void Service_IsPallet_StartWithP()
        //{
        //    // Arrange
        //    var target = new ReceivingService(null, null);

        //    // Act
        //    var actual = target.IsPallet("P123");

        //    Assert.IsTrue(actual);

        //}

        ///// <summary>
        ///// Must return true for pallets eginning with P
        ///// </summary>
        //[TestMethod]
        //public void Service_IsPallet_DoesNotStartWithP()
        //{
        //    // Arrange
        //    var target = new ReceivingService(null, null);

        //    // Act
        //    var actual = target.IsPallet("X123");

        //    Assert.IsFalse(actual);

        //}

        ///// <summary>
        ///// Must return true for pallets eginning with P
        ///// </summary>
        //[TestMethod]
        //public void Service_GetIntranstitCartons_BadCarton()
        //{
        //    // Arrange
        //    var repos = new Mock<IReceivingRepository>(MockBehavior.Loose);
        //    var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
        //    var target = new ReceivingService(repos.Object, session.Object);

        //    // Act
        //    var actual = target.GetIntransitCarton("X123");

        //    Assert.IsNull(actual, "Intransit carton");

        //}


        ///// <summary>
        ///// 
        ///// </summary>
        //[TestMethod]
        //public void Service_GetIntranstitCartons_GoodCarton()
        //{
        //    // Arrange
        //    var repos = new Mock<IReceivingRepository>(MockBehavior.Strict);

        //    repos.Setup(p => p.GetReceivingConfiguration()).Returns(new ReceivingConfiguration
        //    {
                 
        //    });
        //    var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
        //    //session.Setup(p => p[It.IsAny<string>()]).Returns(null);
        //    var target = new ReceivingService(repos.Object, session.Object);
        //    var cartonToReceive = new IntransitCarton
        //    {
        //        CartonId = "123456"
        //    };
        //    repos.Setup(p => p.GetIntransitCarton(cartonToReceive.CartonId)).Returns(cartonToReceive);

        //    // Act
        //    var actual = target.GetIntransitCarton(cartonToReceive.CartonId);

        //    Assert.IsNotNull(actual, "Intransit carton is null");

        //}
        /// <summary>
        /// Test If null pallet Id is passed then Throw Null Reference exception
        /// </summary>
        [Owner("Rajesh")]
        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Service_RecievedCarton_PalletIdNull()
        {
            var intrasitcarton = new IntransitCarton
            {
            };
            //Arrange
            var repos = new Mock<IReceivingRepository>(MockBehavior.Loose);
            var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
            var target = new ReceivingService(repos.Object, session.Object);

            ScanContext ctx = new ScanContext
            {
                 
            };
            // Act
            var actual = target.HandleScan("C1", ctx);
        }

        /// <summary>
        /// Test If null carton to recieve is passed then throw ArgumentNullException
        /// </summary>
        [Owner("Rajesh")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Service_RecievedCarton_CartonToRecieveNull()
        {

            // Arrange
            var repos = new Mock<IReceivingRepository>(MockBehavior.Loose);
            var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
            var target = new ReceivingService(repos.Object, session.Object);
            ScanContext ctx = new ScanContext
            {

            };

            // Act
            var actual = target.HandleScan(null, ctx);
        }


        /// <summary>
        /// Test if recieved date of carton is not null then throw exception ""Carton {0} has already been received"
        /// </summary>
        [Owner("Rajesh")]
        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Service_RecievedCarton_ReceivedDate_NotNull()
        {
            var intrasitcarton = new IntransitCarton
            {
                CartonId = "ASD",
                ReceivedDate = DateTime.Now,
            };
            string str = string.Format("Carton {0} has already been received", intrasitcarton.CartonId);
            // Arrange
            var repos = new Mock<IReceivingRepository>(MockBehavior.Loose);
            var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
            var target = new ReceivingService(repos.Object, session.Object);
            ScanContext ctx = new ScanContext
            {

            };
            // Act
            var actual = target.HandleScan(intrasitcarton.CartonId, ctx);
        }

        ///// <summary>
        ///// if carton received  then get carton must return received carton's disposition Id list.
        /////</summary>
        //[Owner("Ankit")]
        //[TestMethod]
        //public void Service_GetCartonsOnPallet()
        //{
        //    //Arrange
        //    var repos = new Mock<IReceivingRepository>(MockBehavior.Strict);
        //    var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
        //    var palletId = "p12235";
        //    var receivedCartonList = new ReceivedCarton[] { };
        //    repos.Setup(p => p.GetReceivedCartons(palletId, null)).Returns(receivedCartonList);
        //    var target = new ReceivingService(repos.Object, session.Object);

        //    //Act
        //    var actual = target.GetCartonsOnPallet(palletId);

        //    //Assert
        //    Assert.IsNotNull(actual, "Carton Must Not Null");

        //}


        ///// <summary>
        ///// if carton received  then get carton on process must return received carton's disposition list.
        /////</summary>
        //[Owner("Ankit")]
        //[TestMethod]
        //public void Service_GetCartonsOnProcess()
        //{
        //    //Arrange
        //    var repos = new Mock<IReceivingRepository>(MockBehavior.Strict);
        //    var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
        //    var processId = 12235;
        //    var receivedCartonList = new ReceivedCarton[] { };
        //    repos.Setup(p => p.GetReceivedCartons(null, processId)).Returns(receivedCartonList);
        //    var target = new ReceivingService(repos.Object, session.Object);

        //    //Act
        //    var actual = target.GetCartonsOfProcess(processId);

        //    //Assert
        //    Assert.IsNotNull(actual, "Carton Must Not Null");

        //}
        ///// <summary>
        ///// Carton not recieved bcoz dispsition mis match(cartonOnPallet,cartonToReceive)
        ///// </summary>     
        //[TestMethod]
        //public void Service_ReceivedCarton_DifferentDisposition()
        //{
        //    //Arrange
        //    var repos = new Mock<IReceivingRepository>(MockBehavior.Strict);
        //    var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);
        //    var target = new ReceivingService(repos.Object, session.Object);
        //    var intransitCarton = new IntransitCarton
        //    {
        //        DestinationArea = "dstArea",
        //        Building = "build",
        //        SkuDisposition = "skuDisp",
        //        VwhId = "VwhId",
        //        SingleSkuPerPallet = true,
        //        Sku = new Sku
        //        {
        //            SkuId = 1235
        //        }
        //    };
        //    var receivedCartonList = new ReceivedCarton[]{
        //    new ReceivedCarton{
        //        DestinationArea = "garden",
        //        Building = "flat",
        //        SkuDisposition = "testing",
        //        VwhId = "VwhId",
        //        SingleSkuPerPallet = true,
        //        Sku = new Sku
        //        {
        //            SkuId = 1235
        //        },
        //    }
        //    };

        //    repos.Setup(p => p.GetReceivedCartons(It.IsAny<string>(), It.IsAny<int?>())).Returns(receivedCartonList);
        //    string outcome;

        //    // Act
        //    var actual = target.HandleScan(intransitCarton.CartonId, "P123", "D1", 12345, out outcome);
        //    Assert.AreEqual(actual, intransitCarton.DispositionId, "Disposition is different");
        //}
    }
}
