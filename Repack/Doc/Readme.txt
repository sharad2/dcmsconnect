Philosophy
----------
The decision of what to show and what not to show is strictly based on model attributes. No one else is aware of this.
All validations are model based. There is no validation client side script.

The UI selected by the user is saved in a cookie so that next time, the select UI page can be bypassed. Additional checks have been built in to ensure that
the cookie is ignored if the user does not have rights to access the UI.

DropDownList
------------
For each drop down list property, the UIHint("DropDownList") attribute must be specified.
The list values must be provided in another property which has _List suffix. For example:

[DisplayName("Print carton tickets on:")]
[Required(ErrorMessage = "Printer Name is Required")]
[UIHint("DropDownList")]
public string PrinterName { get; set; }

public IDictionary<string, IEnumerable<SelectListItem>> PrinterName_List { get; set; }

Within the view, the list can be displayed like this: @Html.EditorFor(m => m.PrinterName)

Generating Id
Each template will use the id you pass to Editor for such as: @Html.EditorFor(m => m.SewingPlantCode, new { id = "ddlSewingPlant" })

Supporting multiple UI with same model
--------------------------------------
The same model is used to display every UI. The attribute UiHintForUiAttribute is designed to hide/show a field. The attribute RequiredForUi makes a value required
based on the UI. The two attributes can certainly be used together.

This says that the TargetVwhId should be displayed as a drop down list for Conversion, BulkConversion, Advanced and BulkAdvanced. For all other UIs it should be hidden.

        [UiHintForUi(RepackUiStyle.Conversion | RepackUiStyle.BulkConversion | RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced, "DropDownList")]
        public string TargetVwhId { get; set; }

The RequiredForUi attribute enables you to make a field required based on the UI.

		[RequiredForUi(RepackUiStyle.Storage | RepackUiStyle.Receive, ErrorMessage = "Sewing Plant is required")]
        public string SewingPlantCode { get; set; }


Javascript
----------
Repack.partial.js: Hides/Shows the pallet text box depending on the area selected. Hides/Shows the carton id text box depending upon whether carton id needs to be scanned.

CreateDialog.partial.js: Sets up the dialog which opens when the Repack button is clicked. It clears up whatever entries and errors the user may have caused
when the dialog was last opened.

The click handler for the Go button makes an ajax call to the RepackCarton action to create the carton. All data to be passed is in hidden and visible input fields
within two forms: One form on the main page and one form within the dialog.

PieceScan.partial.js: This script implements the front end behavior for incrementing number of scans as a UPC is scanned. It uses the remote validator to
validate the scanned UPC, and has optimization built in to prevent revalidation if the same UPC is scanned.

SkuAutoComplete.unobtrusive.partial.js: Provides autocomplete for UPC. The code is written generically so that it works for both the scanned UPC and the target UPC.



Repack from CAN:(Shiva : 18/7/2012)
------------------------------------
Issues:
-------
1: IALOC_CONTENT table have SSS area and when user select SSS as source area then it also remove pieces of passed Sku from 
   IALOC_CONTENT and remove pieces from MASTER_RAW_INVENTORY in SHL area.
2: Should we also check pieces of carton in SRC_OPEN_CARTON.
