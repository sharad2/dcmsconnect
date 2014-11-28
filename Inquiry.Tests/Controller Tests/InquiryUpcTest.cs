using System.Linq;
using System.Web.Mvc;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using AutoMapper;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{
    /// <summary>
    ///This is a test class for InquiryControllerTest and is intended
    ///to contain all InquiryControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UpcTest
    {
        private TestContext testContextInstance1;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance1;
            }
            set
            {
                testContextInstance1 = value;
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<AreaVwhInventory, AreaVwhInventory>();
            Mapper.CreateMap<Sku, Sku>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }


        /// <summary>
        /// Tests whether UPC scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryUpcScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "UPC",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();

            var target = new HomeController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);

            var expectedScan = "123456";
            // Act
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleUpcScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }

        /// <summary>
        /// Tests whether UPC scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        public void InquiryUpcScanTest_Detail_Valid_Model()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            //Provide reasonable values for most properties
            var expectedSku = new Sku
            {
                Color = "AS",
                Dimension = "L",
                SkuId = 112,
                Style = "ASD",
                SkuSize = "AS",
                //UPC Must Be 12 Digit
                Upc = "123456789012",
                QualityCode = "ASD"

            };

            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Sku>>()))
                .Returns(() => Mapper.Map<Sku, Sku>(expectedSku));

            var expectedAreaVwhInventory = Enumerable.Repeat(
                new AreaVwhInventory
                {
                    InventoryArea = "BIR",
                    InventoryPieces = 12,
                    VwhId = "15"
                },
                1
            ).ToList();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<AreaVwhInventory>>()))
                .Returns(() => Mapper.Map<IList<AreaVwhInventory>, IList<AreaVwhInventory>>(expectedAreaVwhInventory));
            var target = new DetailsController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            // Act
            var result = target.HandleUpcScan("12345");
            // Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Sku>>()), Times.Once(), "Sku should be queried exactly once");
            // Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");
            // Assert for Model values
            Assert.IsInstanceOfType(result, typeof(ViewResult), "View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(SkuInventory));
            var sivm = (SkuInventory)vr.Model;
            Assert.AreEqual(expectedSku.Color, sivm.Sku.Color);
            Assert.AreEqual(expectedSku.Dimension, sivm.Sku.Dimension);
            Assert.AreEqual(expectedSku.SkuId, sivm.Sku.SkuId);
            Assert.AreEqual(expectedSku.SkuSize, sivm.Sku.SkuSize);
            Assert.AreEqual(expectedSku.Style, sivm.Sku.Style);
            Assert.AreEqual(expectedSku.Upc, sivm.Sku.Upc);

            // TODO: Assert other properties of BoxViewModel

            for (int i = 0; i < expectedAreaVwhInventory.Count; ++i)
            {
                Assert.AreEqual(expectedAreaVwhInventory[i].InventoryArea, sivm.Inventory[0].InventoryArea);
                Assert.AreEqual(expectedAreaVwhInventory[i].InventoryPieces, sivm.Inventory[0].InventoryPieces);
                Assert.AreEqual(expectedAreaVwhInventory[i].VwhId, sivm.Inventory[0].VwhId);
            }
        }
        /// <summary>
        /// Tests whether UPC scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        public void InquiryUpcScanTest_Detail_InValid_Model()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            //Provide reasonable values for most properties
            var expectedSku = new Sku
            {
                Color = "AS",
                Dimension = "L",
                SkuId = 112,
                Style = "ASD",
                SkuSize = "AS",
                //UPC is not 12 digit.Thus Invalid
                Upc = "1234",
                QualityCode = "ASD"

            };

            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Sku>>()))
                .Returns(() => Mapper.Map<Sku, Sku>(expectedSku));

            var expectedAreaVwhInventory = Enumerable.Repeat(
                new AreaVwhInventory
                {
                    InventoryArea = "BIR",
                    InventoryPieces = 12,
                    VwhId = "15"
                },
                1
            ).ToList();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<AreaVwhInventory>>()))
                .Returns(() => Mapper.Map<IList<AreaVwhInventory>, IList<AreaVwhInventory>>(expectedAreaVwhInventory));
            var target = new DetailsController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            // Act
            var result = target.HandleUpcScan("12345");
            // Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Sku>>()), Times.Once(), "Sku should be queried exactly once");
            // Assert For Model State
            Assert.IsFalse(target.ModelState.IsValid, "Model State Should Be Valid");
            // Assert for Model values
            Assert.IsInstanceOfType(result, typeof(ViewResult), "View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(SkuInventory));
            var sivm = (SkuInventory)vr.Model;
            var x = ReflectionHelpers.FieldNameFor((SkuInventory m) => m.Sku.Upc);
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((SkuInventory m) => m.Sku.Upc)),
                 "UPC must be invalid");
          }
    }
}
