Sharad Singhal 25 Jul 2011

Inquiry Design goals
 - Every possible type of scan must be recognized.
 - Should be available on both desktop and mobile devices.
 - The user should be alerted to unusual situations such as empty box.
 - All queries should be implemented in a PL/SQL package.
 - Except for jQuery, no dependence on DcmsMobile content.
 - The developer should be able to deduce from the Url which view is being displayed. One way of achieving this is to match view name with action name.
 - Information should be searched for in all tables including history tables.
 - The names of columns retrieved by every query should match the database name of the column.
 - The Maximum row count should be 50 and not exceed the 100 in any case.
 - All the calculations must be in Model or ViewModel, Query should be free from Calculation.

 Desktop Design goals
  - Cross linking should be as prevalent as possible.
  - Minimal use of jquery widgets.
  - Minimum inline styles and classes. Layout classes are permitted.
  - The layout should be fluid. It should accommodate as much of screen width as is available.
  - Whatever is available in the database should be displayed.

Mobile Design goals
 - The layout should be very compact. Only highlights should be displayed.
 - The HTML should be very light to minimize bandwidth requirements. No linking.
 - No jquery widgets at all.

 Sharad 27 Jul 2011:
 - All repository functions must IList<T>. All model collection properties must be of type IList<T>



 Sharad 16 Aug 2011
 ------------------
 Container is different from an Area. An area contains inventory, container contains contents.

 Inventory implies fully qualified SKU which includes Vwh and quality, SKU and pieces.
  Contents minimally include SKU and pieces. Specialized contents may include additional attributes.

Need to Parse
Style, Color, Sewing Plant, quality code, Building, Vwh, Shipment

Box should show MPC Info

Sharad 27 Sep 2011
------------------
Routes:

QualifiedScanRoute class: Enaples URLs matching format /Inquiry/Pickslip/32
Added to routes in global.asax.


ScanRoute class handles incoming URLs matching
/Inquiry/Pickslip/32

The URL matches only if the second component (Pickslip) is one of the enum values defined for InquiryScanType.
32 is treated as the pickslip id and is required. The URL can additionally have two more components pk1 and pk2.



Creating a new Controller:
---------------------------
Derive the controller from InquiryControllerBase.


Add an entry to __mapControllerToMask in class QualifiedScanRoute.

Creating an action in an existing controller:
---------------------------------------------
The ActionName and ScanType attributes must be used as shown below.

[ActionName(QualifiedScanRoute.ACTION_NAME)]
[ScanType(InquiryScanType.PickslipArchived)]
public virtual ActionResult HandlePickslipScan(int id)
{
...
}

Generating action links in views:
---------------------------------
Use ScanRouteLink. It is intelligent enough to generate the link pointing to the current controller.
The link is not generated if the current controller does not support the scan.

@Html.ScanRouteLink(InquiryScanType.Customer, Model.PO.Customer.CustomerId):
@Html.ScanRouteLink(InquiryScanType.PurchaseOrder, Model.PO.PoId, Model.PO.Customer.CustomerId, Model.PO.Iteration.ToString())


Sharad 28 Sep 2011 Meeting Notes
--------------------------------
No way for external apps to request display of archived pickslip.
BUG: What if the pickslip passed is not found by the best controller.

Sharad 10 Jul 2014: Routing URLs
---------------------------------
URLs follow this pattern:

Each URL which handles a particular scan type (ActiveController) is of format /Inquiry/{InquiryScanType}/{id}. E.g. /Inquiry/PO/10193960
URLs for excel files (ExcelController) have the pattern /Inquiry/Excel/{InquiryScanType}/{id}
Autocomplete controller /Inquiry/AutoComplete?...
SummaryController /Inquiry/Summary/...

Limitations: 
  1. InquiryScanType cannot be Excel. If you invent an InquiryScanType = Excel, then this might ambiguity when id = Excel.
  2. InquiryScanType cannot be Autocomplete
  3. InquiryScanType cannot be Summary
  4. No controller can have a name which matches scan type.



