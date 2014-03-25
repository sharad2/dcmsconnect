using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.Helpers
{
    /// <summary>
    /// Provides action selection based on posted form values
    /// </summary>
    /// <remarks>
    /// <para>
    /// You apply this attribute to a controller action or to a controller like this:
    /// <c>[FormActionSelector(typeof(CartonViewModel), "scan", Box.REGEX_UCC)]</c>.
    /// </para>
    /// <para>
    /// We will first enumerate all properties of <c>CartonViewModel</c> which have the
    /// <c>[DataType("scan")]</c>. Then for each property, we will check whether a value exists in the form
    /// whose name is the property name. If no property qualifies, then the action is not selected.
    /// If multiple properties qualify then an exception is raised. If a single property qualifies, then we
    /// apply the regular expression to the form value and return true if the expression matches.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///public class CartonViewModel : MasterModel
    ///{
    ///    [DataType("scan")]
    ///    public string ScannedCartonId { get; set; }
    ///    
    ///    ...
    ///}
    /// ]]>
    /// </code>
    /// <code>
    /// <![CDATA[
    ///[FormActionSelector(typeof(CartonViewModel), "scan", Box.REGEX_UCC)]
    ///[ActionName("Carton")]
    ///[SessionExpireFilter]
    ///public ActionResult AcceptUccInCarton()
    ///{
    ///    var fieldName = ReflectionHelpers.FieldNameFor((CartonViewModel m) => m.ScannedCartonId);
    ///    string cartonId = this.Request.Form[fieldName];
    ///    return RedirectToAction("SkipUcc", "Confirm", new
    ///    {
    ///        uccId = cartonId
    ///    });
    ///}
    /// ]]>
    /// </code>
    /// </example>
    public class FormActionSelectorAttribute : ActionMethodSelectorAttribute
    {
        private readonly Type _modelType;
        private readonly string _propDataType;
        private readonly string _regex;

        public FormActionSelectorAttribute(Type modelType, string propDataType, string regex)
        {
            _modelType = modelType;
            _propDataType = propDataType;
            _regex = regex;
        }
        public string RegularExp
        {
            get
            {
                return _regex;
            }
        }

        public Type ModelType
        {
            get
            {
                return _modelType;
            }
        }

        public string PropertyDataType
        {
            get
            {
                return _propDataType;
            }
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            // This query returns the only property which has the appropriate DataTypeAttribute and for which a value
            // exists in the value provider. Exception if multiple such properties are found
            var prop = _modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttributes(typeof(DataTypeAttribute), true)
                    .Cast<DataTypeAttribute>().Any(q => q.CustomDataType == _propDataType))
                .Where(p => !string.IsNullOrEmpty(controllerContext.HttpContext.Request.Form[p.Name]))
                .SingleOrDefault();
            if (prop == null)
            {
                return false;
            }
            var val = controllerContext.HttpContext.Request.Form[prop.Name];
            if (val.GetType() == typeof(string))
            {
                val = val.Trim().ToUpperInvariant();
            }

            Regex regex = new Regex(_regex);
            return regex.IsMatch(val);
        }
    }
}
