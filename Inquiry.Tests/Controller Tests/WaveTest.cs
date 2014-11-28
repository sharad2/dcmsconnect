using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for WaveTest and is intended
    ///to contain all WaveTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WaveTest
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

        //This Is Used To Initialize  Classes For Providing Only Copied Data.
        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<Wave, Wave>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }

        /// <summary>
        ///A test whether WaveTest Return Expected scan type
        ///</summary>
        [TestMethod()]
        public void InquiryWaveScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "WAV",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo));

            var target = new HomeController();
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);

            var expectedScan = "12334";
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleWaveScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");

        }


        /// <summary>
        /// Tests whether Wave scan returns proper information in the valid model
        ///</summary>
        [TestMethod()]
        public void InquiryWaveTest_Detail_ValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            //Providing Values 
            var expectedWave = new Wave
            {
                AvailableForPitching = "available Pitching",
                BucketId = 12478,
                Comment = "Wave Comment",
                CreatedBy = "creater",
                DateCreated = DateTime.Now,
                Freeze = "freeze",
                Name = "name",
                PitchIaId = "pitch IaId",
                PitchLimit = 120,
                PitchType = "piych Type",
                Status = "wave status"
            };

            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Wave>>()))
             .Returns(() => Mapper.Map<Wave, Wave>(expectedWave)).Verifiable();



            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleWaveScan(expectedWave.BucketId);

            // Assert

            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Wave>>()), Times.Once(), "Wave should be queried exactly once");


            // Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult), "View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model, "Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Wave), "Model IS Not Matching");
            var wvm = (Wave)vr.Model;


            Assert.AreEqual(expectedWave.AvailableForPitching, wvm.AvailableForPitching, "AvailableForPitching");
            Assert.AreEqual(expectedWave.BucketId, wvm.BucketId, "BucketId");
            Assert.AreEqual(expectedWave.Comment, wvm.Comment, "Comment");
            Assert.AreEqual(expectedWave.CreatedBy, wvm.CreatedBy, "CreatedBy");
            Assert.AreEqual(expectedWave.DateCreated, wvm.DateCreated, "DateCreated");
            Assert.AreEqual(expectedWave.Freeze, wvm.Freeze, "Freeze");
            Assert.AreEqual(expectedWave.Name, wvm.Name, "Name");
            Assert.AreEqual(expectedWave.PitchIaId, wvm.PitchIaId, "PitchIaId");
            Assert.AreEqual(expectedWave.PitchLimit, wvm.PitchLimit, "PitchLimit");
            Assert.AreEqual(expectedWave.PitchType, wvm.PitchType, "PitchType");
            Assert.AreEqual(expectedWave.Status, wvm.Status, "Status");
        }
        /// <summary>
        /// Tests whether Wave scan returns proper information in the Invalid model
        ///</summary>
        [TestMethod()]
        public void InquiryWaveTest_Detail_InValidModel()
        {

            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            //Providing Values 
            var expectedWave = new Wave
            {
                AvailableForPitching = "available Pitching",
                BucketId = 12478,
                Comment = "",
                CreatedBy = "creater",
                DateCreated = DateTime.Now,
                Freeze = "freeze",
                Name = "",
                PitchIaId = "pitch IaId",
                PitchLimit = 120,
                PitchType = "piych Type",
                Status = "wave status"
            };

            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Wave>>()))
             .Returns(() => Mapper.Map<Wave, Wave>(expectedWave)).Verifiable();



            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleWaveScan(expectedWave.BucketId);

            // Assert

            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Wave>>()), Times.Once(), "Wave should be queried exactly once");


            // Assert For Model State
            Assert.IsFalse(target.ModelState.IsValid, "Model State Should Be INValid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult), "View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model, "Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Wave), "Model IS Not Matching");
            var wvm = (Wave)vr.Model;

        }

    }
}
