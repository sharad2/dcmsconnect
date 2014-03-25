using System;
using System.Net;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.Tests.Fakes;
using DcmsMobile.BoxPick.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;

namespace DcmsMobile.BoxPick.Tests.Home
{
    /// <summary>
    /// For each action, the session is made minimally valid and we assert that the action gets invoked.
    /// </summary>
    [TestClass]
    public class SessionValidHomeControllerTests
    {
        Pallet _pallet;

        ControllerTestEnvironment<HomeController> _env;
        Mock<IBoxPickRepository> _repos;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _pallet = new Pallet
            {
                PalletId = "P12345",
                QueryTime = DateTime.Now,
                BoxToPick = new Box
                {
                    UccId = "00001234567890123456",
                    SkuInBox = new Sku
                    {
                        SkuId = 123
                    },
                    Pieces = 6,
                    QualityCode = "01",
                    VwhId = "C15",
                    AssociatedCarton = new Carton
                    {
                        CartonId = "VCJ00441671",
                        LocationId = "FFDC282008"
                    }
                },
                CartonSourceArea = "BIR",
                TotalBoxCount = 5,
                PickedBoxCount = 2,
                PickModeText = "ADREPPWSS"
            };

            _env = new ControllerTestEnvironment<HomeController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";

            _repos = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _repos.Object;
        }

        /// <summary>
        /// Pallet must be provided to invoke Carton via GET
        /// </summary>
        [TestMethod]
        public void Session_Valid_Get_Home_Carton()
        {
            _env.RequestMethod = "GET";
            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(_pallet);
            var vr = _env.InvokeAction<ViewResult>("Carton", "AcceptCarton");
        }

        /// <summary>
        /// Pallet must be provided to invoke Carton via POST
        /// </summary>
        [TestMethod]
        public void Session_Valid_Post_Home_Carton()
        {
            _env.RequestMethod = "POST";
            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(_pallet);
            var vr = _env.InvokeAction<ViewResult>("Carton", "AcceptCarton");
            Assert.IsInstanceOfType(vr.Model, typeof(CartonViewModel));
            var model = (CartonViewModel)vr.Model;
            Helpers.AssertPalletMapping(model, _pallet);
        }

        /// <summary>
        /// Pallet and carton must be provided to invoke Carton via GET
        /// </summary>
        [TestMethod]
        public void Session_Valid_Get_Home_Ucc()
        {
            _env.RequestMethod = "GET";
            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(_pallet);
            mm.SetLastCartonAndLocation("12345", null);
            var vr = _env.InvokeAction<ViewResult>("Ucc", "AcceptCartonInUcc");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel));
            var model = (UccViewModel)vr.Model;
            Helpers.AssertPalletMapping(model, _pallet);
        }

        /// <summary>
        /// Pallet and carton must be provided to invoke Carton via POST
        /// </summary>
        [TestMethod]
        public void Session_Valid_Post_Home_Ucc()
        {
            _env.RequestMethod = "POST";
            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(_pallet);
            mm.SetLastCartonAndLocation("12345", null);
            var vr = _env.InvokeAction<ViewResult>("Ucc", "AcceptCartonInUcc");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel));
            var model = (UccViewModel)vr.Model;
            Helpers.AssertPalletMapping(model, _pallet);
        }
    }
}
