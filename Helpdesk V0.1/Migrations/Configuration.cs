namespace Helpdesk_V0._1.Migrations
{
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Collections.Generic;

    internal sealed class Configuration : DbMigrationsConfiguration<Helpdesk_V0._1.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Helpdesk_V0._1.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (!roleManager.RoleExists("Customer"))
            {
                roleManager.Create(new IdentityRole("Customer"));
            }

            var user = new ApplicationUser { UserName = "admin", Email = "Helpdesk@connect-it.com", Company = "All", ExternalId = null, LockoutEnabled = true };

            if (userManager.FindByName(user.UserName) == null)
            {
                var result = userManager.Create(user, "W3lkom!");

                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Admin");
                }
            }
            var Settings = new List<Setting>
            {
                new Setting {Key = "KaseyaURI", Value = "" },
                new Setting {Key = "KaseyaUser", Value = "" },
                new Setting {Key = "KaseyaPassword", Value = "" },
                new Setting {Key = "Helpdesk_Connect_It_KWS_KaseyaWS", Value = "" },
                new Setting {Key = "Helpdesk_Connect_It_SDWS_vsaServiceDeskWS", Value = "" },
                new Setting {Key = "ServiceDeskURI", Value = "" },
                new Setting {Key = "ServiceDeskName", Value = "" },
                new Setting {Key = "ServiceDeskId", Value = "" }
            };

            Settings.ForEach(s => context.Settings.AddOrUpdate(s));
            context.SaveChanges();
        }
    }
}
