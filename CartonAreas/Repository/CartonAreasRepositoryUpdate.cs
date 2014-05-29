using EclipseLibrary.Oracle;
using System;

namespace DcmsMobile.CartonAreas.Repository
{
    /// <summary>
    /// This partial class contains functions which update the database
    /// </summary>
    internal partial class CartonAreasRepository
    {

        /// <summary>
        /// This method will update the capacity and Assigned SKU of passed location
        /// </summary>
        /// <param name="locationId">Contains required properties to update the location</param>
        /// <param name="skuId"></param>
        /// <param name="maxCartons"></param>
        /// <param name="vwhId"></param>
        public int AssignSkuToLocation(string locationId, int? skuId, int? maxCartons, string vwhId)
        {
            const string QUERY = @"
              UPDATE <proxy />MASTER_STORAGE_LOCATION MSL
                 SET MSL.ASSIGNED_SKU_ID      = :SKU_ID,
                     MSL.ASSIGNED_MAX_CARTONS = :CARTON_COUNT,
                     MSL.UNAVAILABLE_FLAG     = :UNAVAILABLE,
                     MSL.ASSIGNED_VWH_ID      = :ASSIGNED_VWH_ID
               WHERE MSL.LOCATION_ID = :LOCATION_ID
            ";
            var binder = SqlBinder.Create()
                .Parameter("LOCATION_ID", locationId)
                .Parameter("SKU_ID", skuId)
                .Parameter("UNAVAILABLE", skuId.HasValue || maxCartons != null || !string.IsNullOrEmpty(vwhId) ? "" : "Y")
                .Parameter("CARTON_COUNT", maxCartons)
                .Parameter("ASSIGNED_VWH_ID", vwhId);
            return _db.ExecuteDml(QUERY, binder);

        }

        /// <summary>
        /// This method is used to update area information.
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// Updates the passed model with updated properties
        /// </remarks>
        public void UpdateArea(CartonArea model)
        {
            const string QUERY = @"
                    UPDATE <proxy />TAB_INVENTORY_AREA TIA
                       SET TIA.LOCATION_NUMBERING_FLAG = :LOCATION_NUMBERING_FLAG,
                           TIA.IS_PALLET_REQUIRED      = :IS_PALLET_REQUIRED,
                           TIA.DESCRIPTION             = :DESCRIPTION,
                           TIA.UNUSABLE_INVENTORY      = :UNUSABLE_INVENTORY
                           WHERE TIA.INVENTORY_STORAGE_AREA =:INVENTORY_STORAGE_AREA
                RETURNING tia.short_name INTO :short_name

";
            var binder = SqlBinder.Create()
            .Parameter("INVENTORY_STORAGE_AREA", model.AreaId)
            .Parameter("LOCATION_NUMBERING_FLAG", model.LocationNumberingFlag ? "Y" : string.Empty)
            .Parameter("IS_PALLET_REQUIRED", model.IsPalletRequired ? "Y" : string.Empty)
            .Parameter("UNUSABLE_INVENTORY", model.UnusableInventory ? "Y" : string.Empty)
            .Parameter("DESCRIPTION", model.Description)
            .OutParameter("short_name", p => model.ShortName = p);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public void UpdatePalletLimit(string buildingId, int? palletLimit)
        {
            const string QUERY = @"
             UPDATE <proxy />TAB_WAREHOUSE_LOCATION T
                   SET T.RECEIVING_PALLET_LIMIT = :RECEIVING_PALLET_LIMIT
                 WHERE T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
            ";
            var binder = SqlBinder.Create()
                .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                .Parameter("RECEIVING_PALLET_LIMIT", palletLimit);
            _db.ExecuteDml(QUERY, binder);

        }

        public void UpdateAddress(string buildingId, string description, Address address)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                throw new ArgumentNullException("BuildingId");
            }
            const string QUERY = @"
                BEGIN
                    UPDATE <proxy />TAB_WAREHOUSE_LOCATION T
                           SET T.DESCRIPTION  = :DESCRIPTION,
                               T.ADDRESS_1    = :ADDRESS_1,
                               T.ADDRESS_2    = :ADDRESS_2,
                               T.ADDRESS_3    = :ADDRESS_3,
                               T.ADDRESS_4    = :ADDRESS_4,
                               T.CITY         = :CITY,
                               T.STATE        = :STATE,
                               T.ZIP_CODE     = :ZIP_CODE,
                               T.COUNTRY_CODE = :COUNTRY_CODE
                         WHERE T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID;
                    IF SQL%ROWCOUNT = 0 THEN
                      RAISE_APPLICATION_ERROR(20000, 'Building ' || :WAREHOUSE_LOCATION_ID || ' not found');
                    END IF;
                END;";
            var binder = SqlBinder.Create()
                .Parameter("DESCRIPTION", description)
                .Parameter("ADDRESS_1", address.Address1)
                .Parameter("ADDRESS_2", address.Address2)
                .Parameter("ADDRESS_3", address.Address3)
                .Parameter("ADDRESS_4", address.Address4)
                .Parameter("CITY", address.City)
                .Parameter("STATE", address.State)
                .Parameter("ZIP_CODE", address.ZipCode)
                .Parameter("COUNTRY_CODE", address.CountryCode)
                .Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public void AddBuilding(Building building)
        {
            const string QUERY = @" 
                    BEGIN
                        INSERT INTO <proxy />TAB_WAREHOUSE_LOCATION T
                                  (T.WAREHOUSE_LOCATION_ID,
                                   T.ADDRESS_1,
                                   T.ADDRESS_2,
                                   T.ADDRESS_3,
                                   T.ADDRESS_4,
                                   T.CITY,
                                   T.STATE,
                                   T.ZIP_CODE,                                   
                                   T.DESCRIPTION,
                                   T.COUNTRY_CODE)
                                VALUES
                                  (:WAREHOUSE_LOCATION_ID,
                                   :ADDRESS_1,
                                   :ADDRESS_2,
                                   :ADDRESS_3,
                                   :ADDRESS_4,
                                   :CITY,
                                   :STATE,
                                   :ZIP_CODE,                                   
                                   :DESCRIPTION,
                                   :COUNTRY_CODE);
                    IF SQL%ROWCOUNT = 0 THEN
                      RAISE_APPLICATION_ERROR(20000, 'Building ' || :WAREHOUSE_LOCATION_ID || ' Not Added.');
                    END IF;
                        END;
                             ";
            var binder = SqlBinder.Create()
                .Parameter("WAREHOUSE_LOCATION_ID", building.BuildingId)
                .Parameter("ADDRESS_1", building.Address.Address1)
                .Parameter("ADDRESS_2", building.Address.Address2)
                .Parameter("ADDRESS_3", building.Address.Address3)
                .Parameter("ADDRESS_4", building.Address.Address4)
                .Parameter("CITY", building.Address.City)
                .Parameter("STATE", building.Address.State)
                .Parameter("ZIP_CODE", building.Address.ZipCode)
                .Parameter("DESCRIPTION", building.Description)
                .Parameter("COUNTRY_CODE", building.Address.CountryCode);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public void UpdatePickingArea(PickingArea pickingArea)
        {
            const string QUERY = @"BEGIN
                                          UPDATE <proxy />IA I
                                             SET I.SHORT_DESCRIPTION  = :SHORT_DESCRIPTION,
                                                 I.SHIPPING_AREA_FLAG = :SHIPPING_AREA_FLAG,
                                                 I.RESOCK_AREA_FLAG   = :RESOCK_AREA_FLAG,
                                                 I.PICKING_AREA_FLAG  = :PICKING_AREA_FLAG
                                           WHERE I.IA_ID = :IA_ID;
                                          IF SQL%ROWCOUNT = 0 THEN
                                            RAISE_APPLICATION_ERROR(20000, 'Area ' || :IA_ID || ' not found');
                                          END IF;
                                        END;";
            var binder = SqlBinder.Create()
                .Parameter("SHORT_DESCRIPTION", pickingArea.Description)
                .Parameter("SHIPPING_AREA_FLAG", pickingArea.IsShippingArea ? "Y" : "")
                .Parameter("RESOCK_AREA_FLAG", pickingArea.IsRestockArea ? "Y" : "")
                .Parameter("PICKING_AREA_FLAG", pickingArea.IsPickingArea ? "Y" : "")
                .Parameter("IA_ID", pickingArea.AreaId);
            _db.ExecuteNonQuery(QUERY, binder);


        }

    }
}