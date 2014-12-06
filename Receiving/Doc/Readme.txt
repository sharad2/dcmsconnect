Sharad 22 Nov 2011
------------------
To support the concept of building, the following changes will be made to the receiving program.

1. The user must specify the building whenever a new receiving process is created.
The choice will be saved in a cookie to avoid having to specify it each time.

2. The user must select the area to receive in. The choices will be all areas within the building for which
tab_inventory_area.is_receiving_area = 'Y'.

3. The user must select the spot check area. We will propose all areas within the building for which 
tab_inventory_area.is_spotcheck_area = 'Y'.

TODO: Can the UI present disposition more meaninfgully?

4. Pallet limit will be at the building level which is the same as today. However, pallet limit will only depend on the building for
which cartons are being received. It will no longer consider which building the carton is meant for. Thus we have effectively got rid of the
label group concept.
This is a behavior change for the edge case where the same shipment contains cartons for multiple buildings. We are desupporting this edge case.
The impact of this desupport is that false warnings will be generated for the secondary buildings. This is a minor consequence with which we should
be able to live with.






 Philosophy
----------
The decision of what to show and what not to show is strictly based on model attributes. No one else is aware of this.
All validations are model based. There is no validation client side script.

The receiving configurations including pallet limit, destination area and spot check area are saved in memory cache for 
30 minutes for efficiency. We use session to store destination area of intransit cartons until they are received.



Javascript
----------
HandleScan.partial.js: Calls HandleScan() method of the controller when user scans something, all data to be passed is hidden in input fieldswithin form with id frmScan.
. The outcome of the scan is indicated by an appropiate 
status code within 200 range. 
 201 (Created): Carton Received. Pallet Html is provided as data. CartonId and Disposition in header. 
 202 (Accepted): Pallet scan. Pallet HTML is provided as data. PalletId and pallet disposition provided as header.
 203 (error): Indicates that some error has occured, show them in screen and play error sound.
 250 (custom): Carton not received due to disposition mismatch. Disposition is the data.


PalletTabs.partial.js: We show all the pallets scanned in the UI in tabs. This file manages adding a new pallet,closing a pallet and unreceiving the carton.
For unreceiving carton a controller method UnreceiveCarton is called. 
 200 (Created): Carton unreceived sucessfully.. 
 203 (error): Indicates that some error has occured, show them in screen and play error sound.


AutoComplete.partial.js: Provides autocomplete for Carrier. (DB TODO: why it is present in two file??)

PrintCarton.partial.js: Provides the dialog for printing the carton ticket. We make an ajax call to fill printer dropdown in run time for efficiency.


Coding conventions being followed 
-------------------------------------------------

- Class names which are used in script must be prefixed with recv-. This helps during refactoring.
- Pallet and carton disposition value is supposed to be opaque. The UI should never assume that it will make sense to the user.

RAD 5 Sep 2011
------------------------
- Pallet Limit should not give delete message



Receiving 13-03-2012
------------------------
- Pallet Limit is not being honored. 
- Ask for building in UI. Once you know of building you can query tab_warehouse_location to get pallet limit.
- Single SKU to a pallet should be at process level.

Receiving 17-03-2012
------------------------
- Pallet Limit will be kept at process level. Keep it in cookie too.(Done)
- Ask for building in UI. Once you know of building you can query tab_warehouse_location to get pallet limit??? (To discuss)
- Single SKU to a pallet should be at process level. (removed)
- Receiving report needs to change.
- Add option to change Price season code in Receiving.(Done)
- Do not use src_carton_prcoess_detail in receiving/

Database- 

- Remove the trigger which updates price_season _code from production. 


---------- DB and Binay ---------------
---------------------------------------
1. Use short name instead of carton_storage_area for display. 
2. 


Receiving 07-06-2013
------------------------
------DB and Ritesh-----

- While creating process receving area drop down will show only those area's which have IS_RECEIVING_AREA flag set in tab_inventory_area table.
- If no area has this flag we will show all areas.
- Spot check area drop down will only show those areas which have IS_SPOTCHECK_AREA flag set.
- In case no area has IS_SPOTCHECK_AREA flag set we show all areas , otherswise we show spotcheck area with in the building of selected receiving area.
- RAD has new feature of enable/disable  configuration settings.
- If any configuration setting has been disbaled then cartons mathcing with these settings will not be sent to SpotCheck area.