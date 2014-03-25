using System.Net;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Tests.Fakes;
using DcmsMobile.BoxPick.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;

namespace DcmsMobile.BoxPick.Tests.Help
{
    /// <summary>
    /// Actions in the HelpController don't expect any input, they returns help view.  
    /// </summary>
    [TestClass]
    public class HelpTests
    {
        private ControllerTestEnvironment<HelpController> _env;

        /// <summary>
        /// Minimally valid pallet must exist in environment. The LastCartonId is set to indicate which carton was scanned.
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HelpController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";
            _env.RequestMethod = "POST";
        }

        [TestMethod]
        public void ShowPalletHelp()
        {
            var vr = _env.InvokeAction<ViewResult>("Pallet", "ShowPalletHelp");
            Assert.IsInstanceOfType(vr.Model, typeof(PalletViewModel), "Unexpected model type");
            Assert.AreEqual("PalletHelp", vr.ViewName, "PalletHelp view to be displayed");

            var model = (PalletViewModel)vr.Model;
            Assert.AreEqual(model.Sound, 'S');
        }

        [TestMethod]
        public void ShowCartonHelp()
        {
            var vr = _env.InvokeAction<ViewResult>("Carton", "ShowCartonHelp");
            Assert.IsInstanceOfType(vr.Model, typeof(CartonViewModel), "Unexpected model type");
            Assert.AreEqual("CartonHelp", vr.ViewName, "CartonHelp view to be displayed");

            var model = (CartonViewModel)vr.Model;
            Assert.AreEqual(model.Sound, 'S');
        }

        [TestMethod]
        public void ShowUccHelp()
        {
            var vr = _env.InvokeAction<ViewResult>("Ucc", "ShowUccHelp");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel), "Unexpected model type");
            Assert.AreEqual("UccHelp", vr.ViewName, "UccHelp view to be displayed");

            var model = (UccViewModel)vr.Model;
            Assert.AreEqual(model.Sound, 'S');
        }

        [TestMethod]
        public void ShowPartialPickPalletHelp()
        {
            var vr = _env.InvokeAction<ViewResult>("PartialPickPallet", "ShowPartialPickPalletHelp");
            Assert.IsInstanceOfType(vr.Model, typeof(PartialPickPalletViewModel), "Unexpected model type");
            Assert.AreEqual("PartialPickPalletHelp", vr.ViewName, "PartialPickPalletHelp view to be displayed");

            var model = (PartialPickPalletViewModel)vr.Model;
            Assert.AreEqual(model.Sound, 'S');
        }

   }
}
