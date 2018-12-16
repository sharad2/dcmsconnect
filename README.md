dcmsconnect
===========

DCMS Connect Source Code.

DCMS is an open source Warehouse Management System which runs on Oracle database.

DCMS connect is a web server which provides
the user interface for DCMS.

# Installation
DCMS Connect has been tested in Visual Studio Community Edition 2017

## Recommended Visual Studio Extensions

* Auto T4MVC http://bennor.github.io/AutoT4MVC/. Automatically updates T4MVC code files as you work.
* Bundler & Minifier https://marketplace.visualstudio.com/items?itemName=MadsKristensen.BundlerMinifier
* File Nesting https://marketplace.visualstudio.com/items?itemName=MadsKristensen.FileNesting
* 

## Known Issues
* 16 Dec 2018: Before the very first build, you must disable Auto T4MVC (Tools -> Options -> Auto T4MVC. Set Run on build and 
Run on Save to false). After a successful build you can turn Auto T4MVC on again. Apparently the build process does some
house keeping which is required before T4MVC can run successfully.

# Sharing _layoutBootstrap file in a project
1. Go to the folder where you want to add this file.
2. Choose Add Existing Item menu.
3. In the file chooser dialog, select the file dcmsconnect\DcmsMobile\MainArea\SharedViews\_layoutBootstrap.cshtml

When debugging this project, we need to copy the linked file, otherwise we will get a runtime error. The below steps
make this happen as described in http://mattperdeck.com/post/Copying-linked-content-files-at-each-build-using-MSBuild.aspx

Edit the project's .csproj file with a text editor. Look for this commented text

````xml
<!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target> -->
````
Add the following text immediately after this comment

````xml
<Target Name="CopyLinkedContentFiles" BeforeTargets="Build">
    <Copy SourceFiles="%(Content.Identity)" 
          DestinationFiles="%(Content.Link)" 
          SkipUnchangedFiles='true' 
          OverwriteReadOnlyFiles='true' 
          Condition="'%(Content.Link)' != ''" />
 </Target>
````

You should also tell git to ignore the copied file so that it does not get stored in version control.


