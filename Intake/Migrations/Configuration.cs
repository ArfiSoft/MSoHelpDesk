namespace Intake.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Intake.Models.SettingsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Intake.Models.SettingsContext context)
        {
            var Settings = new List<Setting>
            {
                new Setting {Key = "KaseyaURI", Value = "" },
                new Setting {Key = "KaseyaUser", Value = "" },
                new Setting {Key = "KaseyaPassword", Value = "" },
                new Setting {Key = "Helpdesk_Connect_It_KWS_KaseyaWS", Value = "" },
                new Setting {Key = "Helpdesk_Connect_It_SDWS_vsaServiceDeskWS", Value = "" },
                new Setting {Key = "ServiceDeskURI", Value = "" }                
            };

            Settings.ForEach(s => context.Settings.AddOrUpdate(s));
            context.SaveChanges();

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
