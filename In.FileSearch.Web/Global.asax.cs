using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using In.FileSearch.Engine;

namespace In.FileSearch.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private IFileSearchManager _manager;

        protected void Application_Start()
        {
            _manager = FileSearchManagerFactory.Get();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        protected void Application_End()
        {
            _manager.Dispose();
        }
    }
}
