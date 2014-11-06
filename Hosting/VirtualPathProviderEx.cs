using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace EclipseLibrary.Mvc.Hosting
{
    /// <summary>
    /// This provider enables you to access files which are outside of the application root dir.
    /// ASP.NET complains if you try to set the url to ~/../Somedir/Somefile. Using this virtual provider, you will
    /// be able to access these files.
    /// 
    /// DCMSMobile uses this capability to access jquery script files from DcmsMobile Scripts director.
    /// </summary>
    /// <remarks>
    /// <para>
    /// VirtualPathProviderEx should only be used in development environment. It enables access to Script and Content folders which would exist under the DcmsMobile project
    /// without having to copy this folder into your own project. It works well with T4MVC.
    /// 
    /// In the constructor of this provider you specify a list of directories which should be redirected to another directory.
    /// </para>
    /// You register this provider in global.asax. The constructor is passed the relative of the alternate application root.
    /// This should probably be the first entry in the Application_Start() function.
    /// <code>
    ///protected void Application_Start()
    ///{
    ///  HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
    ///    Links_Inquiry.Content.Url(),
    ///    Links_Inquiry.Scripts.Url()
    ///  }));
    ///    ...
    ///}
    /// </code>
    /// 
    /// Tell ASP.NET that you would like static files to pass through this VirtualPathProvider by modifying the web.config in the application root.
    /// <code>
    /// <![CDATA[
    ///<system.webServer>
    ///  <!-- Requirement of VirtualPathProviderEx. Needed so that we can access content and script files from DcmsMobile directory-->
    ///  <handlers>
    ///    <add name="AspNetStaticFileHandler-GIF" path="*.gif" verb="GET,HEAD" type="System.Web.StaticFileHandler"/>
    ///    <add name="AspNetStaticFileHandler-JPG" path="*.jpg" verb="GET,HEAD" type="System.Web.StaticFileHandler"/>
    ///    <add name="AspNetStaticFileHandler-CSS" path="*.css" verb="GET,HEAD" type="System.Web.StaticFileHandler"/>
    ///    <add name="AspNetStaticFileHandler-JS" path="*.js" verb="GET,HEAD" type="System.Web.StaticFileHandler"/>
    ///  </handlers>    
    ///</system.webServer>
    /// ]]>
    /// </code>
    /// <para>
    /// For redirecting static files see http://www.paraesthesia.com/archive/2011/07/21/running-static-files-through-virtualpathprovider-in-iis7.aspx.
    /// The static files returned by this VirtualPathProvider are not cached by the browser. This is great for development but horrible for production.
    /// So this provider should only be used under development environment.
    /// </para>
    /// <para>
    /// T4MVC Notes:
    /// 
    /// Even though your Script and Content folders will no longer have any files in them, you must still add the file names you intend to use within your project.
    /// This is so that T4MVC can generate constants for the file names which you will use in your views. Visual Studio will show a warning icon indicating that the files
    /// do not exist, but this is not a problem.
    /// Typically you will add the following nonexistent files to your project:
    /// Content\jquery.mobile-1.4.2.css
    /// Script\jquery-2.1.1.js
    /// Script\jquery.mobile-1.4.2.js
    /// 
    ///   Because .min.js and .min.css files no longer exist in the Content and Script directories of the application, T4MVC will never request minified files.
    ///   This is not good because we want to use minified files in production environment.
    ///   Therefore you should modify the T4MVC.tt.hooks.t4 file so that it requests minified files in production
    ///   
    ///<code>
    ///<![CDATA[
    ///private static string ProcessVirtualPathDefault(string virtualPath) {
    ///    // The path that comes in starts with ~/ and must first be made absolute
    ///    string path = VirtualPathUtility.ToAbsolute(virtualPath);
    ///    // Add your own modifications here before returning the path. ****The code after this should be added by you****
    ///    if (IsProduction())
    ///    {
    ///        if (virtualPath.EndsWith(".js") && !virtualPath.EndsWith(".min.js"))
    ///        {
    ///            path = path.Replace(".js", ".min.js");
    ///        }
    ///        else if (virtualPath.EndsWith(".css") && !virtualPath.EndsWith(".min.css"))
    ///        {
    ///            path = path.Replace(".css", ".min.css");
    ///        }
    ///    }
    ///    return path;
    ///}
    ///]]>
    /// </code>
    /// </para>
    /// </remarks>
    [Obsolete]
    public class VirtualPathProviderEx : VirtualPathProvider
    {
        private class PhysicalVirtualFile : VirtualFile
        {
            private readonly string _physicalPath;

            public PhysicalVirtualFile(string virtualPath, string physicalPath)
                : base(virtualPath)
            {
                _physicalPath = physicalPath;
            }

            public override Stream Open()
            {
                HttpContext.Current.Trace.Write(this.GetType().FullName, string.Format("Opening file at {0} for virtual path {1}", _physicalPath, this.VirtualPath));
                var x = File.Open(_physicalPath, FileMode.Open);
                return x;
            }
        }
        /// <summary>
        /// e.g. "../DcmsMobile"
        /// </summary>
        private readonly string _altAppRootRelative;

        private readonly string[] _dirs;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="altAppRootRelative">The alternate root directory under which we will look for files. This directory should be relative to the root directory
        /// of the application. e.g. ../DcmsMobile</param>
        /// <param name="dirs">The list of directories under app root which need to be mapped to the alternate directory</param>
        /// <remarks>
        /// In gloabal.asax.cs
        /// <code>
        /// </code>
        /// </remarks>
        public VirtualPathProviderEx(string altAppRootRelative, string[] dirs)
        {
            _altAppRootRelative = altAppRootRelative;
            _dirs = dirs;
        }

        /// <summary>
        /// If this virtual path points to a redirectable directory, then check for file existence at te alternate location.
        /// We throw an exception if the file is not found to ease debugging.
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override bool FileExists(string virtualPath)
        {
            if (!IsAlternatePath(virtualPath))
            {
                return Previous.FileExists(virtualPath);
            }
            var physPath = GetAltPhysicalPath(virtualPath);
            if (!File.Exists(physPath))
            {
                throw new FileNotFoundException("File not found " + physPath);
            }
            return true;
        }

        /// <summary>
        /// If this virtual path points to a redirectable directory, then special handle it by returning a special VirtualFile
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (!IsAlternatePath(virtualPath))
            {
                return Previous.GetFile(virtualPath);
            }
            var physPath = GetAltPhysicalPath(virtualPath);
            var x = new PhysicalVirtualFile(virtualPath, physPath);
            return x;
        }


        /// <summary>
        /// This override ensures that changes to _layoutMain.cshtml are visible even without recompiling the project.
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="virtualPathDependencies"></param>
        /// <param name="utcStart"></param>
        /// <returns></returns>
        public override CacheDependency GetCacheDependency(string virtualPath, System.Collections.IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (!IsAlternatePath(virtualPath))
            {
                return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }
            var physPath = GetAltPhysicalPath(virtualPath);
            return new CacheDependency(physPath);
        }

        /// <summary>
        /// If the virtual path begins with any of the directories passed in the constructor, then it is an alternate path
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        private bool IsAlternatePath(string virtualPath)
        {
            var absVirPath = VirtualPathUtility.ToAbsolute(virtualPath);
            return _dirs.Any(p => virtualPath.StartsWith(p) || absVirPath.StartsWith(p));
        }

        /// <summary>
        /// Returns the absolute physical path under alternate application root
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        private string GetAltPhysicalPath(string virtualPath)
        {
            // HostingEnvironment.ApplicationPhysicalPath = "C:\\work\\DcmsMobile\\Inquiry\\"
            // _altAppRootRelative = "../DcmsMobile"
            // virtualPath = "~/Scripts/jquery-2.1.1.js" or "/Scripts/jquery-2.1.1.js" => x = "/Scripts/jquery-2.1.1.js"
            // Return Value: "C:\\work\\DcmsMobile\\Inquiry\\../DcmsMobile//Scripts/jquery-2.1.1.js"
            // The mixture of / and \ in the return value does not seem to bother .NET
            var x = VirtualPathUtility.ToAbsolute(virtualPath);
            return HostingEnvironment.ApplicationPhysicalPath + _altAppRootRelative + "/" + x;
        }

    }



}
