using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AIS.Startup))]
namespace AIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
