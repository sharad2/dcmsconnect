using EclipseLibrary.Oracle;
using System;

namespace DcmsMobile.BoxPick.Repositories
{
    /// <summary>
    /// Contains static methods to create domain entities
    /// </summary>
    public partial class BoxPickRepository
    {
        /// <summary>
        /// Method is used to pick carton 
        /// </summary>
        /// <remarks>
        /// 
        /// When successful:
        /// 1: We update the destination area of box as expected ship_ia_id
        ///and current pieces of boxdet table as the pieces of passed box and carton.
        /// 2: Swap the carton assigned to box with the passed carton.
        /// 3. Delete carton and update audit.
        /// 4. Capture productivity in carton_productivity table.
        /// 5. Capture information in src_open_carton table.
        /// 6. Capture information in src_trasaction table.
        /// 7. capture information in src_trasaction_detail table.
        /// When Unsuccessful:
        /// 1. Passed box is aleady picked or is cancelled.
        /// 2. Passed box does not have same SKU,quantity,quality,vwh as passed carton.
        /// 3. Passed carton does not exist in source storage area(pullcartonarea).
        /// </remarks>
        public void PickCarton(string uccId, string cartonId, DateTime cartonPickStartDate)
        {
            if (string.IsNullOrEmpty(uccId))
            {
                throw new ArgumentNullException("UccId is null");
            }
            if (string.IsNullOrEmpty(cartonId))
            {
                throw new ArgumentNullException("CartonId is null");
            }

            const string QUERY = @" begin
<proxy />pkg_boxpick.pick_carton(aucc128_id => :UccId,
                          acarton_id => :CartonId,
                          aprocess_start_date => :ProcessStartDate);
end;";

            var binder = SqlBinder.Create()
                .Parameter("UccId", uccId)
                .Parameter("CartonId", cartonId)
                .Parameter("ProcessStartDate", cartonPickStartDate);

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// It removes remaining boxes present on the pallet from the pallet so that it can be made
        /// available for other pallet
        /// </summary>
        public int RemoveRemainingBoxesFromPallet(string palletId)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("PalletId is null");
            }

            const string QUERY = @"begin
:result :=  <proxy />pkg_boxpick.remove_pallet_boxes(apallet_id => :PalletId);
end;";
            int boxesRemoved = 0;
            var binder = SqlBinder.Create()
                .Parameter("PalletId", palletId)
                .OutParameter("result", m => boxesRemoved = m ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return boxesRemoved;
        }

        /// <summary>
        /// It removes a box present on the pallet from the pallet so that it can be made
        /// available for other pallet
        /// </summary>
        public void RemoveBoxFromPallet(string uccId, string palletid)
        {
            if (string.IsNullOrEmpty(uccId))
            {
                throw new ArgumentNullException("UccId is null");
            }
            if (string.IsNullOrEmpty(palletid))
            {
                throw new ArgumentNullException("PalletId is null");
            }

            const string QUERY = @"begin
<proxy />pkg_boxpick.remove_box_from_pallet(apallet_id => :PalletId, aucc128_id => :UccId);
end;";
            var binder = SqlBinder.Create()
                .Parameter("UccId", uccId)
                    .Parameter("PalletId", palletid);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// It will create a pallet for building and area specially for ADR pulling
        /// </summary>
        public int CreateADRPallet(string palletId, string buildingId, string area)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("PalletId is null");
            }
            if (string.IsNullOrEmpty(buildingId))
            {
                throw new ArgumentNullException("Building is null");
            }

            const string QUERY = @"
begin
    :result := <proxy />pkg_boxexpedite.create_adr_pallet(apallet_id => :apallet_id,
                                               abuilding => :abuilding,
                                               adestination_area => :adestination_area);
end;
";
            int rowsAffected = 0;
            var binder = SqlBinder.Create().Parameter("apallet_id", palletId)
                .Parameter("abuilding", buildingId)
                .Parameter("adestination_area", area)
                .OutParameter("result", m => rowsAffected = m ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return rowsAffected;
        }


        public void MarkCartonInSuspense(string suspenseCartonId)
        {
            const string QUERY = @"
                update <proxy />src_carton sc set sc.suspense_date = sysdate where sc.carton_id = :SuspenseCartonId
            ";
            var binder = SqlBinder.Create()
                .Parameter("SuspenseCartonId", suspenseCartonId);
            _db.ExecuteNonQuery(QUERY, binder);
        }



    }
}

//$Id$

