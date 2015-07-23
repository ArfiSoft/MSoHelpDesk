using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Helpdesk_V0._1.Startup))]
namespace Helpdesk_V0._1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
