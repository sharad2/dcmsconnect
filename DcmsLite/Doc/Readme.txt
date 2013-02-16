This application template was created by Binod Kumar on 21 Sep 2011.
It implements the Service/Repository pattern and supports globalization.

The mobile page displays a single input control. It always retains focus
and loses its value after each post back.

The desktop page makes an ajax call after each scan and displays the result
returned by the controller.


Steps to convert the DCMS Mobile application template to a Real DCMS MObile application.
[ created by Binod Kumar on 22 Sep 2011.]


1. Create a New folder with name DcmsMobile.Test 
  ['Test' is the name of application, you can use your own application name as Receiving,Boxpick etc..]
2. In this folder check out the DCMS Mobile Application template from 
   path - svn://vcs/net4/Projects/Mvc/DcmsMobile.Starter/trunk 
3. Now open the Solution file [DcmsMobile.Starter.sln] in Visual Studio Editor
4.a. Search the term 'Starter' in Entire Solution and check the option Match Case 
     Now Replace with new Application name as 'Test'
4.b. Save all files, You will see some Error alerts, Just ignore them all.
5. Now Rename the Folder 'Areas/Starter' as 'Areas/Test'
   and change all files name which contains the word 'Starter' to 'Test'as
   'StarterAreaRegistration' to 'TestAreaRegistration'
   'Starter.mobile.partial.css' to 'Test.mobile.partial.css' etc..
6. Change the Assembly name and Default namespace as'DcmsMobile.Test' [By Right Click on appliction 'Starter']
7. Change all .chirp.config files for CSS and JS and now right click on T4MVC.tt file to Run the custom tool
   So that they could recreate all the minified and combined files.
8. Now rename the application from 'Starter' to 'Test'
9. Now close the application.
10. Rename the Folder 'Starter' to 'Test'
    and rename the Solution DcmsMobile.Starter.sln to DcmsMobile.Test.sln
11. Now reopen the Solution file [DcmsMobile.Test.sln] in Visual Studio Editor
    You will see a alert message that one of the projects were not loaded properly. just Ingnore it.
12. Now remove the project 'DcmsMobile.Test' from the solution.
    and add this project again from the ~\DcmsMobile.Test\Test
13. Now right click on T4MVC.tt file and Run the custom tool again.
14. Now Buid the Solution again.

[Updated by Ritesh,Rajesh and Binay]
15. Create new folder in svn with application name like DcmsMobile.Test.
16. Export newly created application in any folder, import this folder to DcmsMobile.Test in svn.
17. END
-------------------------------
Sharad 19 Mar 2012: Updated steps for Renaming
==============================================
Replace Starter with DcmsRights in all files. Replace All will work. Choose Match Case but do not choose Match whole word.
Rename Areas/Starter to Areas/DcmsRights.
In Project Properties, set AssemblyName and Default Namespace to DcmsMobile.DcmsRights.
Rename Views/Starter folder to Views/DcmsRights.
Rename Areas/DcmsRights/Scripts/Starter.partial.js to Rename Areas/DcmsRights/Scripts/DcmsRights.partial.js
Rename Areas/DcmsRights/Content/Starter*.css to DcmsRights*.css
Areas/DcmsRights/Views/Shared/_layoutStarter*.cshtml _layoutDcmsRight*.cshtml

Now right click on T4MVC.tt file and Run the custom tool.

* At this point the application should run directly.

Rename solution from Starter to DcmsRights.

* Now the application should run through DcmsMobile as well.
