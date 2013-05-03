using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.Shipping.ViewModels;

namespace DcmsMobile.Shipping.Areas.Shipping
{
    [MetadataType(typeof(Subareas))]
    public class ShippingAreaRegistration : AreaRegistration
    {
        internal static readonly string COOKIE_PREFIX = typeof(ShippingAreaRegistration).FullName;

        [Display(Name = "Shipping", Order = 100, Description = "Manage order shipping")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "Shipping";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Shipping_default",
                "Shipping/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_Shipping.Shipping.Home.Name,
                    action = MVC_Shipping.Shipping.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(UnroutedViewModel), new UnroutedViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(BolViewModel), new BolViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(AppointmentViewModel), new AppointmentViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(RoutingViewModel), new RoutingViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(RoutedViewModel), new RoutedViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(RoutingSummaryViewModel), new RoutingSummaryViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(LayoutTabsViewModel), new LayoutTabsViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new RoutingPoGroupUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new RoutedgPoGroupUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new UnroutedPoGroupUnbinder());
            ValueProviderFactories.Factories.Add(new CookieValueProviderFactory(COOKIE_PREFIX));
        }

    }

    public class CookieValueProviderFactory : ValueProviderFactory
    {
        private readonly string _cookiePrefix;
        public CookieValueProviderFactory(string cookiePrefix)
        {
            _cookiePrefix = cookiePrefix;
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            return new CookieValueProvider(_cookiePrefix, controllerContext.HttpContext.Request.Cookies);
        }

        private class CookieValueProvider : IValueProvider
        {
            private readonly HttpCookieCollection _cookieCollection;
            private readonly string _cookiePrefix;

            public CookieValueProvider(string cookiePrefix, HttpCookieCollection cookieCollection)
            {
                _cookieCollection = cookieCollection;
                _cookiePrefix = cookiePrefix;
            }

            public bool ContainsPrefix(string prefix)
            {
                return _cookieCollection[_cookiePrefix + prefix] != null;
            }

            public ValueProviderResult GetValue(string key)
            {
                HttpCookie cookie = _cookieCollection[_cookiePrefix + key];
                return cookie != null ?
                  new ValueProviderResult(cookie.Value, cookie.Value, CultureInfo.CurrentUICulture)
                  : null;
            }
        }
    }
}
