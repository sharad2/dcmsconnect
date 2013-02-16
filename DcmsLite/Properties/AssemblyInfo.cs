using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DcmsLite")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems")]
//[assembly: AssemblyProduct("DcmsMobile.DcmsLite")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:


/*Change log
 * Added new connection string in web.config file of DcmsMobile for new schema.
        <add name="dcmslite" connectionString="Data Source=w8ethiopia/dcmsprd1;Proxy User Id=dcmslite;Proxy Password=dcmslite" /> 
 * Added seeting in web.config file of DcmsMobile for dcmslite. 
     <!--This setting is for restock area of LA warehouse, being used in DCMS lite for bulk receiving -->
    <add key="DcmsLite.ReceivingArea" value="RLA" lockItem="true"/>
 * 
 * Change log from 1.0.0.0 -> 1.1.0.0 (Branched by Binod Kumar on 29 JAN 2013)
 * Removed app settings DcmsLite.ReceivingArea, DcmsLite.PrinterName from web.config 
 * Added new setting for warehouse location. [DEPENDENCY]
 * <!--This setting is for warehouse location, being used for DCMS lite -->
    <add key="DcmsLite.WarehouseLocation" value="LA1" lockItem="true"/>
 * Role DCMSLITE_MANAGER is required to run this application [DEPENDENCY]
 * New UI for Batch printing with printer pooling.
 * Reprinting is available for box, batch.
 * Search is implemented to look for Batch, wave, box.
 * Enhanced the UI and process of receiving.
 * 
 * Change log from 1.1.0.0 -> 1.1.1.0 (Branched by Binod Kumar on 11 FEB 2013)
 * Bug Fixed: using oracle SYS.STRAGG() instead of LISTAGG(), Using of LISTAGG() was crossing the predefined limit of Oracle. [Merged changes of Branch 1.1.0.1]
 * Customer filter is available on wave list page and now can show 2000 waves in list
 * If pick wave is frozen, printing option will not be available. 
 * Enhanced the UI of printing. 
 * Showing Box Sequence on Reprint Batch page which will help to identify the missing labels
 * 
 * Change log from 1.1.1.0 -> 1.1.2.0 (Branched by Binod Kumar on 12 FEB 2013)
 * Using spinner control instead of slider for entering the batch size.
 * Storing batch size in cookie and Storing Selected VWH for activating the same tab next time
 * Showing in progress wave in yellow color and completed waves are in green.
 * 
 * Change log from 1.1.2.0 -> 1.2.0.0 (Branched by Binod Kumar on 14 FEB 2013)
 * Box validation is implemented
 * New grants and procedure VALIDATE_BOX_LITE is required [DEPENDENCY]
 */
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]
[assembly: AssemblyProduct(@"
<ol>
<li>New Feature is introduced to validate the boxes.</li>
<li>Customer filter is available on wave list page.</li>
<li>Showing total 2000 waves on wave list page.</li>
<li>If pick wave is frozen, printing option will not be available.</li>
<li>Showing Box Sequence on Reprint Batch page which will help to identify missing labels.</li>
<li>Now using spinner control instead of slider to accept the printing batch size.</li>
<li>Now remembers last entered printing batch size on Print wave page, and last active tab of wave list page.</li>
<li>Showing in progress wave in yellow color and completed waves are in green.</li>
</ol>
")]