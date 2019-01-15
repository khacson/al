using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Alert.Startup))]
namespace Alert
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
