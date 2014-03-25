using System;
using System.Collections.Generic;
using System.Linq;
using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.Tests.Fakes;
using EclipseLibrary.Oracle;
using EclipseLibrary.Oracle.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace DcmsMobile.BoxPick.Tests.Mapping
{
    /// <summary>
    ///This is a test class for BoxPickRepositoryTest and is intended
    ///to contain all BoxPickRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BoxPickRepositoryTest
    {
        #region Creating Entites

        private Pallet CreatePallet()
        {
            return new Pallet()
            {
                PalletId = "P12345",
                QueryTime = DateTime.Now,
                CartonSourceArea = "BIR",
                TotalBoxCount = 5,
                PickedBoxCount = 2,
                DestinationArea = "ADR",
                BuildingId = "FDC",
                PickModeText = "ADREPPWSS",
                BoxToPick = CreateBox()
            };
        }

        private Box CreateBox()
        {
            return new Box()
            {
                AssociatedCarton = CreateCarton(),
                IaId = null,
                Pieces = 36,
                QualityCode = "01",
                SkuInBox = CreateSku()
            };
        }

        private Carton CreateCarton()
        {
            return new Carton()
            {
                CartonId = "VCJ00441671",
                LocationId = "FFDC282008",
                Pieces = 36,
                QualityCode = "01",
                SkuInCarton = CreateSku(),
                StorageArea = "BIR",
                VwhId = "15"
            };
        }

        private Sku CreateSku()
        {
            return new Sku()
            {
                Color = "BK",
                Dimension = "A",
                SkuId = 5,
                SkuSize = "32",
                Style = "08445"
            };
        }

        #endregion

        #region TestContext
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

        #endregion

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

        Mock<IOracleDatastore> _store;
        BoxPickRepository _target;
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _store = new Mock<IOracleDatastore>(MockBehavior.Strict);
            _target = new BoxPickRepository(_store.Object);
        }

        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region NULL Handling Test

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPickContext_Null_Scan_Should_Throw_Exception()
        {
            _target.GetPickContext(string.Empty);
        }

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPalletsToPick_Null_Scan_Should_Throw_Exception()
        {
            _target.GetPalletsToPick(string.Empty, string.Empty);
        }

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCartonDetails_Null_Scan_Should_Throw_Exception()
        {
            _target.GetCartonDetails(string.Empty);
        }

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetBoxesOnPallet_Null_Scan_Should_Throw_Exception()
        {
            _target.GetBoxesOnPallet(string.Empty);
        }

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetrievePalletInfo_Null_Scan_Should_Throw_Exception()
        {
            _target.RetrievePalletInfo(string.Empty);
        }

        /// <summary>
        /// Null scan should throw ArgumentNullException
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RetrieveCartonLocationsForPallet_Null_Scan_Should_Throw_Exception()
        {
            _target.RetrieveCartonLocationsForPallet(string.Empty);
        }
        #endregion

        #region Mapping Test

        [TestMethod()]
        public void PickContext_Mapping()
        {
            PickContext expected = new PickContext()
            {
                BuildingId = "FDC",
                DestinationArea = "ADR"
            };
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"BUILDING_ID", expected.BuildingId},
                            {"DESTINATION_AREA", expected.DestinationArea},
                        };
            var odr = new MockOracleDataRow(dict);
            _store.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<PickContext>>()))
                .Returns((SqlBinder<PickContext> binder) =>
                {
                    return binder.Mapper.Engine.Map<IOracleDataRow, PickContext>(odr);
                });

            PickContext actual = _target.GetPickContext("ADR");

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.BuildingId, actual.BuildingId);
            Assert.AreEqual(expected.DestinationArea, actual.DestinationArea);
        }


        [TestMethod]
        public void GetPalletsToPick_Mapping()
        {
            Pallet expected = CreatePallet();
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"PALLET_ID", expected.PalletId},
                            {"BOX_COUNT", expected.TotalBoxCount},
                            {"PICKABLE_BOX_COUNT", expected.PickableBoxCount},
                            {"PICKED_BOX_COUNT", expected.PickedBoxCount},
                            {"RESERVED_CARTON_COUNT", expected.ReservedCartonCount},
                            {"PICKMODE", expected.PickModeText},
                            {"WAREHOUSE_LOCATION_ID", expected.BuildingId},
                            {"DESTINATION_AREA", expected.DestinationArea}
                        };
            var odr = new MockOracleDataRow(dict);
            _store.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Pallet>>()))
                .Returns((SqlBinder<Pallet> binder) =>
                {
                    var result = binder.Mapper.Engine.Map<IOracleDataRow, Pallet>(odr);
                    return new Pallet[] { result };
                });

            var actual = _target.GetPalletsToPick("FDC", string.Empty).ToArray();
            
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.BuildingId, actual[0].BuildingId);
            Assert.AreEqual(expected.PickModeText, actual[0].PickModeText);
            Assert.AreEqual(expected.DestinationArea, actual[0].DestinationArea);
            Assert.AreEqual(expected.PalletId, actual[0].PalletId);
            Assert.AreEqual(expected.TotalBoxCount, actual[0].TotalBoxCount);
            Assert.AreEqual(expected.PickableBoxCount, actual[0].PickableBoxCount);
            Assert.AreEqual(expected.PickedBoxCount, actual[0].PickedBoxCount);
        }

        [TestMethod()]
        public void GetCartonDetails_Mapping()
        {
            Carton expected = CreateCarton();
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"carton_id", expected.CartonId},
                            {"location_id", expected.LocationId},
                            {"QUANTITY", expected.Pieces},
                            {"QUALITY_CODE", expected.QualityCode},
                            {"carton_storage_area", expected.StorageArea},
                            {"VWH_ID", expected.VwhId},

                            {"color", expected.SkuInCarton.Color},
                            {"dimension", expected.SkuInCarton.Dimension},
                            {"SKU_ID", expected.SkuInCarton.SkuId},
                            {"sku_size", expected.SkuInCarton.SkuSize},
                            {"style", expected.SkuInCarton.Style}
                        };
            var odr = new MockOracleDataRow(dict);
            _store.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Carton>>()))
                .Returns((SqlBinder<Carton> binder) =>
                {
                    return binder.Mapper.Engine.Map<IOracleDataRow, Carton>(odr);
                });

            Carton actual = _target.GetCartonDetails(expected.CartonId);

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.CartonId, expected.CartonId);
            Assert.AreEqual(actual.LocationId, expected.LocationId);
            Assert.AreEqual(actual.Pieces, expected.Pieces);
            Assert.AreEqual(actual.QualityCode, expected.QualityCode);
            Assert.AreEqual(actual.StorageArea, expected.StorageArea);
            Assert.AreEqual(actual.VwhId, expected.VwhId);

            Assert.IsNotNull(actual.SkuInCarton);
            Assert.AreEqual(actual.SkuInCarton.Color, expected.SkuInCarton.Color);
            Assert.AreEqual(actual.SkuInCarton.Dimension, expected.SkuInCarton.Dimension);
            Assert.AreEqual(actual.SkuInCarton.SkuId, expected.SkuInCarton.SkuId);
            Assert.AreEqual(actual.SkuInCarton.SkuSize, expected.SkuInCarton.SkuSize);
            Assert.AreEqual(actual.SkuInCarton.Style, expected.SkuInCarton.Style);
        }


        [TestMethod()]
        public void GetBoxesOnPallet_Mapping()
        {
            Box expected = CreateBox();
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"ucc128_id", expected.UccId},
                            {"expected_pieces", expected.Pieces},
                            {"quality_code", expected.QualityCode},
                            {"ia_id", expected.IaId},
                            {"vwh_id", expected.VwhId},

                            {"color", expected.SkuInBox.Color},
                            {"dimension", expected.SkuInBox.Dimension},
                            {"SKU_ID", expected.SkuInBox.SkuId},
                            {"sku_size", expected.SkuInBox.SkuSize},
                            {"style", expected.SkuInBox.Style},

                            {"carton_location_id", expected.AssociatedCarton.LocationId},
                            {"carton_carton_id", expected.AssociatedCarton.CartonId},

                            {"carton_color", expected.AssociatedCarton.SkuInCarton.Color},
                            {"carton_dimension", expected.AssociatedCarton.SkuInCarton.Dimension},
                            {"carton_SKU_ID", expected.AssociatedCarton.SkuInCarton.SkuId},
                            {"carton_sku_size", expected.AssociatedCarton.SkuInCarton.SkuSize},
                            {"carton_style", expected.AssociatedCarton.SkuInCarton.Style}
                        };
            var odr = new MockOracleDataRow(dict);

            _store.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()))
                .Returns((SqlBinder<Box> binder) =>
                {
                    var result = binder.Mapper.Engine.Map<IOracleDataRow, Box>(odr);
                    return new Box[] { result };
                });

            var actual = _target.GetBoxesOnPallet("P12345").ToArray();

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual[0].UccId, expected.UccId);
            Assert.AreEqual(actual[0].Pieces, expected.Pieces);
            Assert.AreEqual(actual[0].QualityCode, expected.QualityCode);
            Assert.AreEqual(actual[0].IaId, expected.IaId);
            Assert.AreEqual(actual[0].VwhId, expected.VwhId);

            Assert.IsNotNull(actual[0].SkuInBox);
            Assert.AreEqual(actual[0].SkuInBox.Color, expected.SkuInBox.Color);
            Assert.AreEqual(actual[0].SkuInBox.Dimension, expected.SkuInBox.Dimension);
            Assert.AreEqual(actual[0].SkuInBox.SkuId, expected.SkuInBox.SkuId);
            Assert.AreEqual(actual[0].SkuInBox.SkuSize, expected.SkuInBox.SkuSize);
            Assert.AreEqual(actual[0].SkuInBox.Style, expected.SkuInBox.Style);

            Assert.IsNotNull(actual[0].AssociatedCarton);
            Assert.AreEqual(actual[0].AssociatedCarton.CartonId, expected.AssociatedCarton.CartonId);
            Assert.AreEqual(actual[0].AssociatedCarton.LocationId, expected.AssociatedCarton.LocationId);

            Assert.IsNotNull(actual[0].AssociatedCarton.SkuInCarton);
            Assert.AreEqual(actual[0].AssociatedCarton.SkuInCarton.Color, expected.AssociatedCarton.SkuInCarton.Color);
            Assert.AreEqual(actual[0].AssociatedCarton.SkuInCarton.Dimension, expected.AssociatedCarton.SkuInCarton.Dimension);
            Assert.AreEqual(actual[0].AssociatedCarton.SkuInCarton.SkuId, expected.AssociatedCarton.SkuInCarton.SkuId);
            Assert.AreEqual(actual[0].AssociatedCarton.SkuInCarton.SkuSize, expected.AssociatedCarton.SkuInCarton.SkuSize);
            Assert.AreEqual(actual[0].AssociatedCarton.SkuInCarton.Style, expected.AssociatedCarton.SkuInCarton.Style);
        }

        [TestMethod()]
        public void RetrieveCartonLocationsForPallet_Mapping()
        {
            CartonLocation expected = new CartonLocation
            {
                CartonLocationId = "FF01",
                CartonStorageArea = "BIR",
                CountCartonsToPick = 10,
                PiecesPerCarton = 10,
                SkuToPick = CreateSku()
            };
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"carton_location_id", expected.CartonLocationId},
                            {"carton_storage_area", expected.CartonStorageArea},
                            {"count_cartons_to_pick", expected.CountCartonsToPick},
                            {"pieces_per_carton", expected.PiecesPerCarton},

                            {"color", expected.SkuToPick.Color},
                            {"dimension", expected.SkuToPick.Dimension},
                            {"SKU_ID", expected.SkuToPick.SkuId},
                            {"sku_size", expected.SkuToPick.SkuSize},
                            {"style", expected.SkuToPick.Style},
                        };
            var odr = new MockOracleDataRow(dict);

            _store.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<CartonLocation>>()))
                .Returns((SqlBinder<CartonLocation> binder) =>
                {
                    var result = binder.Mapper.Engine.Map<IOracleDataRow, CartonLocation>(odr);
                    return new CartonLocation[] { result };
                });

            var actual = _target.RetrieveCartonLocationsForPallet("P12345").ToArray();

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual[0].CartonLocationId, expected.CartonLocationId);
            Assert.AreEqual(actual[0].CartonStorageArea, expected.CartonStorageArea);
            Assert.AreEqual(actual[0].CountCartonsToPick, expected.CountCartonsToPick);
            Assert.AreEqual(actual[0].PiecesPerCarton, expected.PiecesPerCarton);

            Assert.IsNotNull(actual[0].SkuToPick);
            Assert.AreEqual(actual[0].SkuToPick.Color, expected.SkuToPick.Color);
            Assert.AreEqual(actual[0].SkuToPick.Dimension, expected.SkuToPick.Dimension);
            Assert.AreEqual(actual[0].SkuToPick.SkuId, expected.SkuToPick.SkuId);
            Assert.AreEqual(actual[0].SkuToPick.SkuSize, expected.SkuToPick.SkuSize);
            Assert.AreEqual(actual[0].SkuToPick.Style, expected.SkuToPick.Style);
        }

        [TestMethod()]
        public void RetrievePalletInfo_Mapping()
        {
            Pallet expected = CreatePallet();
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            {"PALLET_ID", expected.PalletId},
                            {"BOX_COUNT", expected.TotalBoxCount},
                            {"PICKABLE_BOX_COUNT", expected.PickableBoxCount},
                            {"PICKED_BOX_COUNT", expected.PickedBoxCount},
                            {"RESERVED_CARTON_COUNT", expected.ReservedCartonCount},
                            {"WAREHOUSE_LOCATION_ID", expected.BuildingId},
                            {"DESTINATION_AREA", expected.DestinationArea},
                            {"PICK_MODE", expected.PickModeText},
                            {"CARTONPICKSTARTDATE", expected.QueryTime},
                            {"CARTON_SOURCE_AREA", expected.CartonSourceArea},

                            {"BOX_UCC128_ID", expected.BoxToPick.UccId},
                            {"BOX_EXPECTED_PIECES", expected.BoxToPick.Pieces},
                            {"BOX_QUALITY_CODE", expected.BoxToPick.QualityCode},
                            {"BOX_IA_ID", expected.BoxToPick.IaId},
                            {"BOX_VWHID", expected.BoxToPick.VwhId},

                            {"BOX_color", expected.BoxToPick.SkuInBox.Color},
                            {"BOX_dimension", expected.BoxToPick.SkuInBox.Dimension},
                            {"BOX_SKU_ID", expected.BoxToPick.SkuInBox.SkuId},
                            {"BOX_sku_size", expected.BoxToPick.SkuInBox.SkuSize},
                            {"BOX_style", expected.BoxToPick.SkuInBox.Style},

                            {"BOX_carton_location_id", expected.BoxToPick.AssociatedCarton.LocationId},
                            {"BOX_carton_carton_id", expected.BoxToPick.AssociatedCarton.CartonId},

                            {"BOX_CARTON_VWHID", expected.BoxToPick.AssociatedCarton.VwhId},
                            {"BOX_CARTON_QUALITYCODE", expected.BoxToPick.AssociatedCarton.QualityCode},

                            {"BOX_CARTON_color", expected.BoxToPick.AssociatedCarton.SkuInCarton.Color},
                            {"BOX_CARTON_dimension", expected.BoxToPick.AssociatedCarton.SkuInCarton.Dimension},
                            {"BOX_CARTON_SKU_ID", expected.BoxToPick.AssociatedCarton.SkuInCarton.SkuId},
                            {"BOX_CARTON_sku_size", expected.BoxToPick.AssociatedCarton.SkuInCarton.SkuSize},
                            {"BOX_CARTON_style", expected.BoxToPick.AssociatedCarton.SkuInCarton.Style}
                        };
            var odr = new MockOracleDataRow(dict);

            _store.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pallet>>()))
                .Returns((SqlBinder<Pallet> binder) =>
                {
                    return binder.Mapper.Engine.Map<IOracleDataRow, Pallet>(odr);
                });

            RetrieveCartonLocationsForPallet_Mapping();
            var actual = _target.RetrievePalletInfo(expected.PalletId);

            Assert.AreEqual(actual.PalletId, expected.PalletId);
            Assert.AreEqual(actual.PickableBoxCount, expected.PickableBoxCount);
            Assert.AreEqual(actual.PickedBoxCount, expected.PickedBoxCount);
            Assert.AreEqual(actual.PickModeText, expected.PickModeText);
            Assert.AreEqual(actual.QueryTime, expected.QueryTime);
            Assert.AreEqual(actual.ReservedCartonCount, expected.ReservedCartonCount);
            Assert.AreEqual(actual.TotalBoxCount, expected.TotalBoxCount);
            Assert.AreEqual(actual.BuildingId, expected.BuildingId);
            Assert.AreEqual(actual.CartonSourceArea, expected.CartonSourceArea);
            Assert.AreEqual(actual.DestinationArea, expected.DestinationArea);

            Assert.IsNotNull(actual.BoxToPick);
            Assert.AreEqual(actual.BoxToPick.UccId, expected.BoxToPick.UccId);
            Assert.AreEqual(actual.BoxToPick.Pieces, expected.BoxToPick.Pieces);
            Assert.AreEqual(actual.BoxToPick.QualityCode, expected.BoxToPick.QualityCode);
            Assert.AreEqual(actual.BoxToPick.IaId, expected.BoxToPick.IaId);
            Assert.AreEqual(actual.BoxToPick.VwhId, expected.BoxToPick.VwhId);

            Assert.IsNotNull(actual.BoxToPick.SkuInBox);
            Assert.AreEqual(actual.BoxToPick.SkuInBox.Color, expected.BoxToPick.SkuInBox.Color);
            Assert.AreEqual(actual.BoxToPick.SkuInBox.Dimension, expected.BoxToPick.SkuInBox.Dimension);
            Assert.AreEqual(actual.BoxToPick.SkuInBox.SkuId, expected.BoxToPick.SkuInBox.SkuId);
            Assert.AreEqual(actual.BoxToPick.SkuInBox.SkuSize, expected.BoxToPick.SkuInBox.SkuSize);
            Assert.AreEqual(actual.BoxToPick.SkuInBox.Style, expected.BoxToPick.SkuInBox.Style);

            Assert.IsNotNull(actual.BoxToPick.AssociatedCarton);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.CartonId, expected.BoxToPick.AssociatedCarton.CartonId);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.LocationId, expected.BoxToPick.AssociatedCarton.LocationId);

            Assert.IsNotNull(actual.BoxToPick.AssociatedCarton.SkuInCarton);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.SkuInCarton.Color, expected.BoxToPick.AssociatedCarton.SkuInCarton.Color);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.SkuInCarton.Dimension, expected.BoxToPick.AssociatedCarton.SkuInCarton.Dimension);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.SkuInCarton.SkuId, expected.BoxToPick.AssociatedCarton.SkuInCarton.SkuId);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.SkuInCarton.SkuSize, expected.BoxToPick.AssociatedCarton.SkuInCarton.SkuSize);
            Assert.AreEqual(actual.BoxToPick.AssociatedCarton.SkuInCarton.Style, expected.BoxToPick.AssociatedCarton.SkuInCarton.Style);
        }

        #endregion
    }
}
