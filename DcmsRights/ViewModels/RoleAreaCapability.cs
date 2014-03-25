using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
//using EclipseLibrary.Mvc.Areas;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.DcmsRights.ViewModels
{
    /// <summary>
    /// Represents the capability provided by a specific role in a specific area
    /// </summary>
    /// <remarks>
    /// <para>
    /// </para>
    /// <para>
    /// IEquatable interface enforces equality by comparing RoleName and AreaName only. Key attribute is used for documentation purposes only.
    /// </para>
    /// </remarks>
    internal class RoleAreaCapability : IEquatable<RoleAreaCapability>
    {
        private string _rolename;

        [Key]
        public string RoleName
        {
            get
            {
                return _rolename;
            }
            set
            {
                _rolename = value.ToUpper();
            }
        }

        [Key]
        public string AreaName { get; set; }

        public string AreaDescription { get; set; }

        public string Purpose { get; set; }

        public bool Equals(RoleAreaCapability other)
        {
            return this.RoleName == other.RoleName && this.AreaName == this.AreaName;
        }

        public override int GetHashCode()
        {
            return this.RoleName.GetHashCode() + this.AreaName.GetHashCode();
        }

        /// <summary>
        /// Returns the capabilies of the area. Caches the capabilities with a sliding expiration. The cache deppends on the xml file so that
        /// updated contents can be read if the xml file changess.
        /// </summary>
        /// <remarks>
        /// We specifically exclude this app by excluding the DcmsRightsAreaRegistration area. We do not want anyone to be able to give rights to this program.
        /// </remarks>
        public static IEnumerable<RoleAreaCapability> Capabilities
        {
            get
            {
                var capabilities = MemoryCache.Default[typeof(RoleAreaCapability).FullName] as IEnumerable<RoleAreaCapability>;

                if (capabilities == null)
                {
                    // Sharad 23 Nov 2012: Now parsing AuthorizeEx applied to Controllers
                    // This query should be equivalent to the above query but it does not use PluggableAreas2
                    var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                where !assembly.GlobalAssemblyCache
                                let types = assembly.GetTypes()
                                from type in types
                                where typeof(Controller).IsAssignableFrom(type) && type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true).Length == 0
                                let attrDisplay = types.Where(p => typeof(AreaRegistration).IsAssignableFrom(p))
                                    .Select(p => p.GetProperty("AreaName"))
                                    .SelectMany(p => p.GetCustomAttributes(typeof(DisplayAttribute), false))
                                    .Cast<DisplayAttribute>().FirstOrDefault()
                                where attrDisplay != null
                                from method in type.GetMethods()
                                from attr in (method.GetCustomAttributes(typeof(AuthorizeExAttribute), false).Concat(type.GetCustomAttributes(typeof(AuthorizeExAttribute), false)))
                                    .Cast<AuthorizeExAttribute>()
                                from roleName in attr.Roles.Split(',')
                                select new RoleAreaCapability
                                {
                                    AreaName = attrDisplay.Name,
                                    AreaDescription = attrDisplay.Description,
                                    Purpose = attr.Purpose,
                                    RoleName = roleName
                                };

                    var xmlfile = HttpContext.Current.Server.MapPath("~/App_Data/LegacyProgramRoles.xml");
                    var root = XElement.Load(xmlfile);

                    var legacyPrograms = from program in root.Elements("program")
                                         from role in program.Elements("role")
                                         select new RoleAreaCapability
                                         {
                                             AreaName = (string)program.Attribute("name"),
                                             AreaDescription = (string)program.Attribute("description"),
                                             RoleName = (string)role.Attribute("name"),
                                             Purpose = (string)role.Attribute("purpose")
                                         };

                    capabilities = query.Concat(legacyPrograms).Distinct().ToList();

                    var item = new CacheItem(typeof(RoleAreaCapability).FullName, capabilities);
                    var policy = new CacheItemPolicy();
                    policy.SlidingExpiration = TimeSpan.FromMinutes(30);

                    // Discard the cache if the file changes
                    policy.ChangeMonitors.Add(new HostFileChangeMonitor(new[] { xmlfile }));

                    MemoryCache.Default.Add(typeof(RoleAreaCapability).FullName, capabilities, policy);
                }


                return capabilities;
            }
        }
    }
}