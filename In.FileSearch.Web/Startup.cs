using System;
using In.FileSearch.Engine;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(In.FileSearch.Web.Startup))]
namespace In.FileSearch.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
