using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    /// <summary>
    /// Base class for editing VAS configuration UI
    /// </summary>
    public abstract class EditVasModelBase
    {

        private static KeyedByTypeCollection<object> __listEnumMeta;
        protected static IDictionary<T, DisplayAttribute> GetEnumDisplayAttributes<T>()
        {
            if (__listEnumMeta == null)
            {
                __listEnumMeta = new KeyedByTypeCollection<object>();
            }
            if (!__listEnumMeta.Contains(typeof(Dictionary<T, DisplayAttribute>)))
            {
                IDictionary<T, DisplayAttribute> query = (from T item in Enum.GetValues(typeof(T))
                                                          select new
                                                          {
                                                              Value = item.GetType().GetMember(item.ToString())
                                                                               .Cast<MemberInfo>().Single().GetCustomAttributes(typeof(DisplayAttribute), false)
                                                                               .Cast<DisplayAttribute>()
                                                                               .SingleOrDefault(),
                                                              Key = item
                                                          }).Where(p => p.Value != null).ToDictionary(p => p.Key, p => p.Value);
                __listEnumMeta.Add(query);
            }
            return (IDictionary<T, DisplayAttribute>)__listEnumMeta[typeof(Dictionary<T, DisplayAttribute>)];
        }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// VAS ID for which customer created a pattern
        /// </summary>
        [Display(Name = "VAS Type")]
        public string VasId { get; set; }

        /// <summary>
        /// Descriptive text of VAS ID
        /// </summary>
        public string VasDescription { get; set; }

        /// <summary>
        /// Comma separated list of labels to which the VAS will be applied
        /// </summary>
        [Display(Name = "Labels")]
        public string Labels { get; set; }

        /// <summary>
        /// StartWith, EndsWith, etc.
        /// </summary>
        public VasPoPatternType? PoPatternType { get; set; }

        public VasPoTextType? PoTextType { get; set; }

        public string PoText { get; set; }

        /// <summary>
        /// Currently applied RegEx for VAS pattern
        /// </summary>
        public string PatternRegEx { get; set; }

        /// <summary>
        /// Description of VAS pattern <see cref="PatternRegEx"/> which is already applied for customer (System generated)
        /// </summary>
        public string RegExDescription { get; set; }

        /// <summary>
        /// Constructs the VasPatternDescription based on current values
        /// </summary>
        /// <returns></returns>
        public string ConstructedRegExDescription
        {
            get
            {
                var labelList = new string[] { };
                if (!string.IsNullOrWhiteSpace(Labels))
                {
                    labelList = Labels.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                }

                if (labelList.Length == 0 && PoPatternType == null)
                {
                    return "All Labels";
                }

                var sb = new StringBuilder();
                if (PoPatternType == null)
                {
                    sb.AppendFormat("All purchase orders");
                }
                else
                {
                    sb.AppendFormat("Purchase Order {0} ", GetEnumDisplayAttributes<VasPoPatternType>()[this.PoPatternType.Value].Name);
                }
                sb.AppendFormat("{0}", PoTextType != null ? GetEnumDisplayAttributes<VasPoTextType>()[PoTextType.Value].Name : PoText);
                if (labelList.Length != 0)
                {
                    sb.AppendFormat(" of Labels {0}", string.Join(", ", Labels));
                }
                else
                {
                    sb.AppendFormat(" of All Labels");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// RegEx that will be constructed based on current values
        /// </summary>
        public string ConstructedRegEx
        {
            get
            {
                {
                    string poFormatString;
                    string poText;
                    if (this.PoPatternType == null)
                    {
                        poFormatString = ".*";
                        poText = ".*";
                    }
                    else
                    {
                        poFormatString = GetEnumDisplayAttributes<VasPoPatternType>()[PoPatternType.Value].GroupName;
                        if (PoTextType == null)
                        {
                            poText = PoText;
                        }
                        else
                        {
                            poText = GetEnumDisplayAttributes<VasPoTextType>()[PoTextType.Value].GroupName;
                        }

                    }

                    var poPattern = string.Format(poFormatString, poText);
                    string labelPattern;
                    if (string.IsNullOrEmpty(Labels))
                    {
                        labelPattern = ".*";
                    }
                    else
                    {
                        labelPattern = string.Format("({0})$", string.Join("|", Labels.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray()));
                    }
                    return poPattern + "@" + labelPattern;
                }
            }
        }
    }
}