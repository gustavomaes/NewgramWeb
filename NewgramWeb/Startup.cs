using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NewgramWeb.Startup))]
namespace NewgramWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
