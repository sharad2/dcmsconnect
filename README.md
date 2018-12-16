dcmsconnect
===========

DCMS Connect Source Code.

DCMS is an open source Warehouse Management System which runs on Oracle database.

DCMS connect is a web server which provides
the user interface for DCMS.

Installation
=============
Visual Studio Community Edition 2017 with the following extensions is required for compiling and running this solution:

* Auto T4MVC http://bennor.github.io/AutoT4MVC/

Known Issues
============
16 Dec 2018: Before the very first build, you must disable Auto T4MVC (Tools -> Options -> Auto T4MVC. Set Run on build and 
Run on Save to false). After a successful build you can turn Auto T4MVC on again. Apparently the build process does some
house keeping which is required before T4MVC can run successfully.

