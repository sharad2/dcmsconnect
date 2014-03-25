
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.BoxPick.ViewModels;
using DcmsMobile.BoxPick.Models;

namespace DcmsMobile.BoxPick.Tests
{
    public static class Helpers
    {
        public static void AssertPalletMapping(MasterModelWithPallet model, Pallet pallet)
        {
            if (pallet == null)
            {
                Assert.IsNull(model.CurrentPalletId);
                Assert.IsNull(model.ProductivityStartTime);
                Assert.IsNull(model.CartonSourceAreaToPick);
                Assert.AreEqual(0, model.PalletPickableBoxCount);
                Assert.AreEqual(0, model.PalletPickedBoxCount);
                Assert.AreEqual(0, model.PalletTotalBoxCount);
            }
            else
            {
                Assert.AreEqual(model.CurrentPalletId, pallet.PalletId);
                Assert.AreEqual(model.ProductivityStartTime, pallet.QueryTime);
                Assert.AreEqual(model.CartonSourceAreaToPick, pallet.CartonSourceArea);
                Assert.AreEqual(model.PalletPickableBoxCount, pallet.PickableBoxCount);
                Assert.AreEqual(model.PalletPickedBoxCount, pallet.PickedBoxCount);
                Assert.AreEqual(model.PalletTotalBoxCount, pallet.TotalBoxCount);
            }

            if (pallet == null || pallet.BoxToPick == null)
            {
                Assert.IsNull(model.PiecesToPick);
                Assert.IsNull(model.QualityCodeToPick);
                Assert.IsNull(model.VwhIdToPick);
                Assert.IsNull(model.UccIdToPick);
                Assert.IsNull(model.SkuIdToPick);
                Assert.IsNull(model.SkuDisplayNameToPick);
                Assert.IsNull(model.CartonIdToPick);
            }
            else
            {
                Assert.AreEqual(model.PiecesToPick, pallet.BoxToPick.Pieces);
                Assert.AreEqual(model.QualityCodeToPick, pallet.BoxToPick.QualityCode);
                Assert.AreEqual(model.VwhIdToPick, pallet.BoxToPick.VwhId);
                Assert.AreEqual(model.UccIdToPick, pallet.BoxToPick.UccId);


            }

            if (pallet == null || pallet.BoxToPick == null || pallet.BoxToPick.SkuInBox == null)
            {
                Assert.IsNull(model.SkuIdToPick);
                Assert.IsNull(model.SkuDisplayNameToPick);
            }
            else
            {
                Assert.AreEqual(model.SkuIdToPick, pallet.BoxToPick.SkuInBox.SkuId);
                Assert.AreEqual(model.SkuDisplayNameToPick, pallet.BoxToPick.SkuInBox.DisplayName);
            }

            if (pallet == null || pallet.BoxToPick == null || pallet.BoxToPick.AssociatedCarton == null)
            {
                Assert.IsNull(model.CartonIdToPick);
            }
            else
            {
                Assert.AreEqual(model.CartonIdToPick, pallet.BoxToPick.AssociatedCarton.CartonId);
            }
        }
    }
}
