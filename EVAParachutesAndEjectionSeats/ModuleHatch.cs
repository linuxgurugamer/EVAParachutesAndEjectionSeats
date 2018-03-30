using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if false
namespace VanguardTechnologies
{
    public class ModuleKrHatch : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Hatch active")]
        public bool isActiveHatch = false;

        [KSPEvent(guiActive = true, guiName = "Activate hatch")]
        public void ActivateHatch()
        {
            foreach (ModuleKrHatch h in GetHatches(vessel))
                h.isActiveHatch = false;
            this.isActiveHatch = true;
        }

        [KSPEvent(guiActive = true, guiName = "Start EVA")]
        public void StartEVA()
        {
            ModuleKrCrewCompartment c = ModuleKrCrewCompartment.GetCompartments(vessel).FirstOrDefault(x => x.part.protoModuleCrew.Count>0);
            if (c == null)
                ScreenMessages.PostScreenMessage("No crew compartment with crew found", 3, ScreenMessageStyle.UPPER_CENTER);
            else
            {
                ProtoCrewMember m = c.part.protoModuleCrew[0];
                c.part.RemoveCrewmember(m);
                part.AddCrewmember(m);
                FlightEVA.fetch.spawnEVA(m, part, part.airlock);
            }
        }

        public override void OnUpdate()
        {
            if (part.protoModuleCrew.Count > 0)
            {
                List<ModuleKrCrewCompartment> clist = ModuleKrCrewCompartment.GetCompartments(vessel);
                foreach (ModuleKrCrewCompartment c in clist)
                    if (c.HasFreeSeat)
                    {
                        ProtoCrewMember cmember = part.protoModuleCrew[0];
                        part.RemoveCrewmember(cmember);
                        c.part.AddCrewmember(cmember);
                        vessel.SpawnCrew();
                        break;
                    }
            }

            if (vessel.isActiveVessel)
                part.CrewCapacity = 1;
            else
                part.CrewCapacity = ModuleKrCrewCompartment.GetCompartments(vessel).Any(c => c.HasFreeSeat) ? 1 : 0;

        }

        public override void OnStart(PartModule.StartState state)
        {
            part.CrewCapacity = 0; //WhyTF does this not remove crew spaces in the editor?
        }

        public static List<ModuleKrHatch> GetHatches(Vessel v)
        {
            List<ModuleKrHatch> hList = new List<ModuleKrHatch>();
            foreach (Part p in v.parts)
                foreach (PartModule pm in p.Modules)
                    if (pm is ModuleKrHatch)
                        hList.Add((ModuleKrHatch)pm);
            return hList;
        }
    }
}
#endif