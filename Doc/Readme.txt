Desktop Application
===================

The primary objective of this app is allow SKU and capacity assignments to carton area locations.

Screen 1d: Displays list of carton areas grouped by building. For each area shows whether the location is numbered, and the number of locations in the area.
Clicking on the area displays Screen 2 (locations in an area).

Screen 2d: Displays locations of a specific area. Filters are available to see assigned or unassigned locations. Another filter to see locations containing fewer than n Cartons.
Max 100 locations will be displayed per screen. Sorting is by travel sequence. Each location displays current number of cartons, capacity, SKU (assigned and actual),
total pieces, number of pallets. Option to edit location is available which leads to Screen 3 which is an ajax dialog. Option to quick assign locations is also available
which leads to Screen 4.

Screen 3d: Dialog to edit location. Ability to edit assigned SKU and carton capacity.But if the location is already assigned and contain cartons  then we allow assignment of slu and max carton 
on location but wita a warning message, and the replenishment will not done until the cartons of previously assigned sku is completly picked.

Screen 4 (common for mobile and desktop): Quick assign locations. Scan location, scan SKU, enter carton capacity. Scan location again to confirm. Repeat for each location.

Mobile Application
==================
Screen 1m: Displays number of assigned/unassigned locations for each numbered carton area. Enter 1 to quick assign locations which leads to Screen 4. Blank enter exits.

Integration with Inquiry
========================
When carton location scanned, display assigned SKU/carton capacity. (Issue #245)
When carton area is scanned, display number of assigned/unassigned locations.

