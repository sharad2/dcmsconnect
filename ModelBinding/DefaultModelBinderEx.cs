using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.ModelBinding
{
    /// <summary>
    /// Provides useful enhancements to the model binding process
    /// </summary>
    /// <remarks>
    /// <para>
    /// To use this model binder, the global.asax file must set it as the default binder.
    /// </para>
    /// <code>
    /// <![CDATA[
    ///protected void Application_Start()
    ///{
    ///    AreaRegistration.RegisterAllAreas();
    ///    RegisterGlobalFilters(GlobalFilters.Filters);
    ///    RegisterRoutes(RouteTable.Routes);
    ///    ModelBinders.Binders.DefaultBinder = new DefaultModelBinderEx();
    ///}
    /// ]]>
    /// </code>
    /// <para>
    /// Feature 1: Automatically convert user input to upper case. Apply the <see cref="BindUpperCaseAttribute"/> to the property which should always receive
    /// input converted to uppercase.
    /// </para>
    /// </remarks>
    public class DefaultModelBinderEx:DefaultModelBinder
    {
        /// <summary>
        /// If the property has the <see cref="BindUpperCaseAttribute"/> applied, convert the passed value to upper case.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        /// <param name="value"></param>
        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.Attributes.OfType<BindUpperCaseAttribute>().Any())
            {
                var str = value as string;
                if (!string.IsNullOrWhiteSpace(str))
                {
                    value = str.ToUpper();
                }
            }
            base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
        }
    }
}
