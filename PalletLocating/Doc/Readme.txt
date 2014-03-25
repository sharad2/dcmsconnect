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
   path - svn://vcs/net4/Projects/Mvc/DcmsMobile.PalletLocating/trunk 
3. Now open the Solution file [DcmsMobile.PalletLocating.sln] in Visual Studio Editor
4.a. Search the term 'PalletLocating' in Entire Solution and check the option Match Case 
     Now Replace with new Application name as 'Test'
4.b. Save all files, You will see some Error alerts, Just ignore them all.
5. Now Rename the Folder 'Areas/PalletLocating' as 'Areas/Test'
   and change all files name which contains the word 'PalletLocating' to 'Test'as
   'PalletLocatingAreaRegistration' to 'TestAreaRegistration'
   'PalletLocating.mobile.partial.css' to 'Test.mobile.partial.css' etc..
6. Change the Assembly name and Default namespace as'DcmsMobile.Test' [By Right Click on appliction 'PalletLocating']
7. Now right click on T4MVC.tt file and Run the custom tool also change the all .chirp.config files for CSS and JS
   So that they could recreate all the minified and combined files.
8. Now rename the application from 'Startup' to 'Test'
9. Now close the application.
10. Rename the Folder 'PalletLocating' to 'Test'
11. Now reopen the Solution file [DcmsMobile.Test.sln] in Visual Studio Editor
    You will see a alert message that one of the projects were not loaded properly. just Ingnore it.
12. Now remove the project 'DcmsMobile.Test' from the solution.
    and add this project again from the ~\DcmsMobile.Test\Test
13. Now right click on T4MVC.tt file and Run the custom tool again.
14. Now Buid the Solution again.

15. END