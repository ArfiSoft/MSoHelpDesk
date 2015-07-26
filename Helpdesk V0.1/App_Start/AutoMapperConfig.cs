using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Helpdesk_V0._1.App_Start
{
    public class AutoMapperConfig
    {
        public static void Config()
        {
            Mapper.CreateMap<SDWS.IncidentSummary, Models.IncidentSummaryGridModel>().IgnoreAllPropertiesWithAnInaccessibleSetter();
            Mapper.CreateMap<SDWS.Incident, Models.IncidentModel>().IgnoreAllPropertiesWithAnInaccessibleSetter();
        }
    }
}