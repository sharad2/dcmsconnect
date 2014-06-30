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
        public string AssignSkuToLocation(string locationId, int? skuId, int? maxCartons, string vwhId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                throw new ArgumentNullException("locationId", "Location ID can not be null");
            }
            const string QUERY = @"
              UPDATE <proxy />MASTER_STORAGE_LOCATION MSL
                 SET MSL.ASSIGNED_SKU_ID      = :SKU_ID,
                     MSL.ASSIGNED_MAX_CARTONS = :CARTON_COUNT,
                     MSL.UNAVAILABLE_FLAG     = :UNAVAILABLE,
                     MSL.ASSIGNED_VWH_ID      = :ASSIGNED_VWH_ID
               WHERE MSL.LOCATION_ID = :LOCATION_ID
                RETURNING MSL.STORAGE_AREA INTO :STORAGE_AREA
            ";
            string areaId = null;
            var binder = SqlBinder.Create()
                .Parameter("LOCATION_ID", locationId)
                .Parameter("SKU_ID", skuId)
                .Parameter("UNAVAILABLE", skuId.HasValue || maxCartons != null || !string.IsNullOrEmpty(vwhId) ? "" : "Y")
                .Parameter("CARTON_COUNT", maxCartons)
                .Parameter("ASSIGNED_VWH_ID", vwhId)
                .OutParameter("STORAGE_AREA", p => areaId = p);
            _db.ExecuteDml(QUERY, binder);
            return areaId;
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
                DECLARE
                  LSHORT_NAME            VARCHAR(3);
                  LWAREHOUSE_LOCATION_ID VARCHAR(5);
                BEGIN
                    UPDATE <proxy />TAB_INVENTORY_AREA TIA
                       SET TIA.LOCATION_NUMBERING_FLAG = :LOCATION_NUMBERING_FLAG,
                           TIA.IS_PALLET_REQUIRED      = :IS_PALLET_REQUIRED,
                           TIA.DESCRIPTION             = :DESCRIPTION,
                           TIA.UNUSABLE_INVENTORY      = :UNUSABLE_INVENTORY
                           WHERE TIA.INVENTORY_STORAGE_AREA =:INVENTORY_STORAGE_AREA
                  RETURNING TIA.SHORT_NAME, TIA.WAREHOUSE_LOCATION_ID INTO LSHORT_NAME, LWAREHOUSE_LOCATION_ID;
                END;

";
            var binder = SqlBinder.Create()
            .Parameter("INVENTORY_STORAGE_AREA", model.AreaId)
            .Parameter("LOCATION_NUMBERING_FLAG", model.LocationNumberingFlag ? "Y" : string.Empty)
            .Parameter("IS_PALLET_REQUIRED", model.IsPalletRequired ? "Y" : string.Empty)
            .Parameter("UNUSABLE_INVENTORY", model.UnusableInventory ? "Y" : string.Empty)
            .Parameter("DESCRIPTION", model.Description)
            .OutParameter("LSHORT_NAME", p => model.ShortName = p)
            .OutParameter("LWAREHOUSE_LOCATION_ID", p => model.BuildingId = p);
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

        public void AddBuilding(string buildingId, string description, Address address)
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
                .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                .Parameter("ADDRESS_1", address.Address1)
                .Parameter("ADDRESS_2", address.Address2)
                .Parameter("ADDRESS_3", address.Address3)
                .Parameter("ADDRESS_4", address.Address4)
                .Parameter("CITY", address.City)
                .Parameter("STATE", address.State)
                .Parameter("ZIP_CODE", address.ZipCode)
                .Parameter("DESCRIPTION", description)
                .Parameter("COUNTRY_CODE", address.CountryCode);
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
                                           WHERE I.IA_ID = :IA_ID
                                            RETURNING I.short_name INTO :short_name;
                                          IF SQL%ROWCOUNT = 0 THEN
                                            RAISE_APPLICATION_ERROR(20000, 'Area ' || :IA_ID || ' not found');
                                          END IF;
                                        END;";
            var binder = SqlBinder.Create()
                .Parameter("SHORT_DESCRIPTION", pickingArea.Description)
                .Parameter("SHIPPING_AREA_FLAG", pickingArea.IsShippingArea ? "Y" : "")
                .Parameter("RESOCK_AREA_FLAG", pickingArea.IsRestockArea ? "Y" : "")
                .Parameter("PICKING_AREA_FLAG", pickingArea.IsPickingArea ? "Y" : "")
                .Parameter("IA_ID", pickingArea.AreaId)
                .OutParameter("short_name", p => pickingArea.ShortName = p);
            _db.ExecuteNonQuery(QUERY, binder);


        }

    }
}