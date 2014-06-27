using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.Models
{
    [Flags]
    internal enum PlatformFlags
    {
        Default = 0x0,

        /// <summary>
        /// The referenced program is optimized for mobile devices
        /// </summary>
        MobileOptimized = 0x1,

        /// <summary>
        /// The referenced program is optimized for desktop devices
        /// </summary>
        DesktopOptimized = 0x2
    }

    ///// <summary>
    ///// The key of the area item is ShortName
    ///// </summary>
    //[Obsolete]
    //public class AreaItemCollection : KeyedCollection<string, AreaItem>
    //{
    //    private static IEnumerable<Type> GetExportedTypesExceptionSafe(Assembly ass)
    //    {
    //        try
    //        {
    //            return ass.GetExportedTypes();
    //        }
    //        catch (TypeLoadException ex)
    //        {
    //            // Trace then ignore
    //            if (HttpContext.Current != null && HttpContext.Current.Trace != null)
    //            {
    //                HttpContext.Current.Trace.Warn("GetPublicTypesOfAssembly", string.Format("DCMS Mobile was unable to include the assembly {0} within its menu", ass.FullName), ex);
    //            }
    //            return Enumerable.Empty<Type>();
    //        }
    //    }


    //    private readonly bool _initializing;
    //    /// <summary>
    //    /// Populate the collection when constructed
    //    /// </summary>
    //    public AreaItemCollection()
    //    {
    //        _initializing = true;
    //        // Sharad 9 AUg 2013: Ignore signed assemblies
    //        var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
    //                    where !assembly.GlobalAssemblyCache && !assembly.IsDynamic && assembly.GetName().GetPublicKey().Length == 0
    //                    let attrProduct = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().FirstOrDefault()
    //                    from type in GetExportedTypesExceptionSafe(assembly)
    //                    where typeof(AreaRegistration).IsAssignableFrom(type) && !type.IsAbstract
    //                    let metadataAttr = type.GetCustomAttributes(typeof(MetadataTypeAttribute), false).Cast<MetadataTypeAttribute>().FirstOrDefault()
    //                    let allAttr = type.GetProperty("AreaName").GetCustomAttributes(false)
    //                    let displayAttr = allAttr.OfType<DisplayAttribute>().FirstOrDefault()
    //                    where displayAttr != null
    //                    let area = (AreaRegistration)Activator.CreateInstance(type)
    //                    let displayFormatAttr = allAttr.OfType<DisplayFormatAttribute>().FirstOrDefault()
    //                    select new
    //                    {
    //                        MetadataClassType = metadataAttr == null ? null : metadataAttr.MetadataClassType,
    //                        Area = new AreaItem
    //                        {
    //                            Description = displayAttr.Description,
    //                            Name = displayAttr.Name,
    //                            Platforms = (allAttr.OfType<UIHintAttribute>().Any(p => p.PresentationLayer == "DcmsMobile" && p.UIHint == "mobile") ?
    //                               PlatformFlags.MobileOptimized : PlatformFlags.Default) |
    //                               (allAttr.OfType<UIHintAttribute>().Any(p => p.PresentationLayer == "DcmsMobile" && p.UIHint == "desktop") ?
    //                               PlatformFlags.DesktopOptimized : PlatformFlags.Default),
    //                            AreaName = area.AreaName,
    //                            Order = displayAttr.GetOrder() ?? 0,
    //                            //ScanUrlFormatString = displayFormatAttr == null ? string.Empty : displayFormatAttr.DataFormatString,
    //                            ChangeLog = attrProduct == null ? string.Empty : attrProduct.Product,
    //                            PostDate = System.IO.File.GetCreationTime(assembly.Location),
    //                            ShortName = displayAttr.ShortName,
    //                            RouteValues = new RouteValueDictionary {
    //                                {"area", area.AreaName}
    //                         }
    //                        }
    //                    };

    //        foreach (var item in query)
    //        {
    //            if (item.MetadataClassType != null)
    //            {
    //                item.Area.SubAreas = (from prop in item.MetadataClassType.GetProperties(BindingFlags.Public | BindingFlags.Static)
    //                                      let allAttr = prop.GetCustomAttributes(false).Cast<Attribute>()
    //                                      let displayAttr = allAttr.OfType<DisplayAttribute>().FirstOrDefault()
    //                                      let uihintAttr = allAttr.OfType<UIHintAttribute>().Where(p => p.PresentationLayer == "DcmsMobile")
    //                                      where displayAttr != null
    //                                      select new AreaItem
    //                                      {
    //                                          Description = displayAttr.Description,
    //                                          Name = displayAttr.Name,
    //                                          Order = displayAttr.GetOrder() ?? 0,
    //                                          Platforms = (uihintAttr.Any(p => p.UIHint == "mobile") ? PlatformFlags.MobileOptimized : PlatformFlags.Default) |
    //                                             (uihintAttr.Any(p => p.UIHint == "desktop") ? PlatformFlags.DesktopOptimized : PlatformFlags.Default),
    //                                          ShortName = displayAttr.ShortName,
    //                                          RouteValues = (RouteValueDictionary)prop.GetValue(null, null)
    //                                      }).ToArray();
    //            }
    //            Add(item.Area);
    //        }
    //        _initializing = false;
    //    }

    //    protected override string GetKeyForItem(AreaItem item)
    //    {
    //        return item.ShortName;
    //    }

    //    protected override void InsertItem(int index, AreaItem item)
    //    {
    //        if (!_initializing)
    //        {
    //            throw new NotSupportedException("This is a readonly collection");
    //        }
    //        base.InsertItem(index, item);
    //    }

    //    protected override void ClearItems()
    //    {
    //        if (!_initializing)
    //        {
    //            throw new NotSupportedException("This is a readonly collection");
    //        }
    //        base.ClearItems();
    //    }

    //    protected override void RemoveItem(int index)
    //    {
    //        if (!_initializing)
    //        {
    //            throw new NotSupportedException("This is a readonly collection");
    //        }
    //        base.RemoveItem(index);
    //    }

    //    protected override void SetItem(int index, AreaItem item)
    //    {
    //        if (!_initializing)
    //        {
    //            throw new NotSupportedException("This is a readonly collection");
    //        }
    //        base.SetItem(index, item);
    //    }
    //}

    public class AreaItem
    {
        /// <summary>
        /// Link text. [Display(Name="")]
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the link. [Display(Description="")]
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Ideal platforms for the link. Mobile or desktop. 
        ///   [UIHint("mobile", "DcmsMobile")]
        ///   [UIHint("desktop", "DcmsMobile")]
        /// </summary>
        internal PlatformFlags Platforms { get; set; }

        public bool IsMobileOptimized
        {
            get
            {
                return this.Platforms.HasFlag(PlatformFlags.MobileOptimized);
            }
        }

        public bool IsDesktopOptimized
        {
            get
            {
                return this.Platforms.HasFlag(PlatformFlags.DesktopOptimized);
            }
        }

        /// <summary>
        /// The placement of the area in the menu list. [Display(Order=100)]
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// If the user enters this, we select the area. [Display(ShortName="")]
        /// </summary>
        public string ShortName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.ShortName, this.Name);
        }

        /// <summary>
        /// The route values which can be used to construct the URL to this area as in 
        /// var url = Url.RouteUrl(area.RouteValues),
        /// </summary>
        public RouteValueDictionary RouteValues { get; set; }

        #region RC Properties
        /// <summary>
        /// AssemblyProduct attribute applied to the area assembly
        /// </summary>
        public string ChangeLog { get; set; }

        /// <summary>
        /// Creation time of the DLL which contains the code for the area
        /// </summary>
        public DateTime? PostDate { get; set; }
        #endregion

        /// <summary>
        /// Name of the area. Obtained by evaluating property AreaRegistration.AreaName
        /// </summary>
        internal string AreaName { get; set; }

        public IEnumerable<AreaItem> SubAreas { get; set; }

        private readonly static Lazy<IList<AreaItem>> __areaItems = new Lazy<IList<AreaItem>>(() => GenerateAreaItems());

        internal static IList<AreaItem> Areas
        {
            get
            {
                return __areaItems.Value;
            }
        }

        /// <summary>
        /// This UniqueId is used to match the items on RC website with the items on the main website. For this reason, this id must be generated such that it will evaluate to the
        /// same value on both websites. We are using the pattern AreaName_ShortName
        /// </summary>
        public string UniqueId { get; set; }

        private static IList<AreaItem> GenerateAreaItems()
        {
            // Sharad 9 AUg 2013: Ignore signed assemblies
            var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where !assembly.GlobalAssemblyCache && !assembly.IsDynamic && assembly.GetName().GetPublicKey().Length == 0
                        let attrProduct = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().FirstOrDefault()
                        from type in GetExportedTypesExceptionSafe(assembly)
                        where typeof(AreaRegistration).IsAssignableFrom(type) && !type.IsAbstract
                        let metadataAttr = type.GetCustomAttributes(typeof(MetadataTypeAttribute), false).Cast<MetadataTypeAttribute>().FirstOrDefault()
                        let allAttr = type.GetProperty("AreaName").GetCustomAttributes(false)
                        let displayAttr = allAttr.OfType<DisplayAttribute>().FirstOrDefault()
                        where displayAttr != null
                        let area = (AreaRegistration)Activator.CreateInstance(type)
                        let displayFormatAttr = allAttr.OfType<DisplayFormatAttribute>().FirstOrDefault()
                        select new
                        {
                            MetadataClassType = metadataAttr == null ? null : metadataAttr.MetadataClassType,
                            Area = new AreaItem
                            {
                                Description = displayAttr.Description,
                                Name = displayAttr.Name,
                                Platforms = (allAttr.OfType<UIHintAttribute>().Any(p => p.PresentationLayer == "DcmsMobile" && p.UIHint == "mobile") ?
                                   PlatformFlags.MobileOptimized : PlatformFlags.Default) |
                                   (allAttr.OfType<UIHintAttribute>().Any(p => p.PresentationLayer == "DcmsMobile" && p.UIHint == "desktop") ?
                                   PlatformFlags.DesktopOptimized : PlatformFlags.Default),
                                AreaName = area.AreaName,
                                Order = displayAttr.GetOrder() ?? 0,
                                //ScanUrlFormatString = displayFormatAttr == null ? string.Empty : displayFormatAttr.DataFormatString,
                                ChangeLog = attrProduct == null ? string.Empty : attrProduct.Product,
                                PostDate = System.IO.File.GetCreationTime(assembly.Location),
                                ShortName = displayAttr.ShortName,
                                RouteValues = new RouteValueDictionary {
                                    {"area", area.AreaName}
                             }
                            }
                        };

            var list = new List<AreaItem>();

            foreach (var item in query)
            {
                if (item.MetadataClassType != null)
                {
                    item.Area.SubAreas = (from prop in item.MetadataClassType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                          let allAttr = prop.GetCustomAttributes(false).Cast<Attribute>()
                                          let displayAttr = allAttr.OfType<DisplayAttribute>().FirstOrDefault()
                                          let uihintAttr = allAttr.OfType<UIHintAttribute>().Where(p => p.PresentationLayer == "DcmsMobile")
                                          where displayAttr != null
                                          select new AreaItem
                                          {
                                              Description = displayAttr.Description,
                                              Name = displayAttr.Name,
                                              Order = displayAttr.GetOrder() ?? 0,
                                              Platforms = (uihintAttr.Any(p => p.UIHint == "mobile") ? PlatformFlags.MobileOptimized : PlatformFlags.Default) |
                                                 (uihintAttr.Any(p => p.UIHint == "desktop") ? PlatformFlags.DesktopOptimized : PlatformFlags.Default),
                                              ShortName = displayAttr.ShortName,
                                              RouteValues = (RouteValueDictionary)prop.GetValue(null, null)
                                          }).ToArray();
                }
                list.Add(item.Area);
            }

            // Assigne a unique sequence to each area item
            var seq = 0;
            foreach (var item in list.Where(p => p.SubAreas != null).SelectMany(p => p.SubAreas).Concat(list))
            {
                item.UniqueId = string.Format("{0}_{1}", item.AreaName, item.ShortName);
                ++seq;
            }
            return list;
        }

        private static IEnumerable<Type> GetExportedTypesExceptionSafe(Assembly ass)
        {
            try
            {
                return ass.GetExportedTypes();
            }
            catch (TypeLoadException ex)
            {
                // Trace then ignore
                if (HttpContext.Current != null && HttpContext.Current.Trace != null)
                {
                    HttpContext.Current.Trace.Warn("GetPublicTypesOfAssembly", string.Format("DCMS Mobile was unable to include the assembly {0} within its menu", ass.FullName), ex);
                }
                return Enumerable.Empty<Type>();
            }
        }

    }
}