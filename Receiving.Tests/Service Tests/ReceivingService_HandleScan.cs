using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.Receiving.Repository;
using Moq;
using System.Web;
using DcmsMobile.Receiving.Models;
using AutoMapper;

namespace Receiving.Tests.Service_Tests
{
    /// <summary>
    /// Summary description for ReceivingService_HandleScan
    /// </summary>
    [TestClass]
    public class ReceivingService_HandleScan
    {
        public ReceivingService_HandleScan()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

        /// <summary>
        /// Scenario: Valid carton (i.e. it exists in intransit) passed to HandleScan and it gets received.
        /// </summary>
        [TestMethod]
        public void Service_HandleScan_ValidIntransitCarton()
        {
            // Arrange
            Mapper.CreateMap<IntransitCarton, IntransitCarton>();
            var intransitCarton = new IntransitCarton
            {
                CartonId = "123"
               
            };
            var config = new ReceivingConfiguration
            {
                ReceivingArea = "RCV",
                SpotCheckArea = "AWL"
            };
            var ctx = new ScanContext
            {
                PalletId = "P123",
                ProcessId = 123
                
            };

            var repos = new Mock<IReceivingRepository>(MockBehavior.Strict);
            repos.Setup(p => p.GetIntransitCarton(intransitCarton.CartonId)).Returns(Mapper.Map<IntransitCarton, IntransitCarton>(intransitCarton));
            repos.Setup(p => p.GetReceivingConfiguration()).Returns(config);
            repos.Setup(p => p.ReceiveCarton(ctx.PalletId, intransitCarton.CartonId, config.ReceivingArea, ctx.ProcessId));            
            repos.Setup(p => p.GetReceivedCartons(ctx.PalletId, null, null, null)).Returns(new [] {
                new ReceivedCarton {
                     CartonId = intransitCarton.CartonId
                }
            });
            //repos.Setup(p => p.getp
            
            var session = new Mock<HttpSessionStateBase>(MockBehavior.Loose);

            var target = new ReceivingService(repos.Object, session.Object);


            // Act
            var actual = target.HandleScan(intransitCarton.CartonId, ctx);

            // Assert
            Assert.AreEqual(ScanResult.CartonReceived, ctx.Result);
            //TODO: More asserts
        }
    }
}
