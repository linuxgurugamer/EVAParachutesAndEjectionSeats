using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if false
namespace VanguardTechnologies
{
    public class ModuleKrEquipKerbal : PartModule
    {
        [KSPField]
        public int range = 1;

        [KSPField]
        public string guiName = "Equip";

        [KSPField(guiActive = true, guiName = "Available", isPersistant = true)]
        public float count = 10;

        public ConfigNode moduleNode = null;

        public override void OnStart(PartModule.StartState state)
        {
            Events["EquipNearbyKerbal"].unfocusedRange = range;
            if (count < 0)
                Fields["count"].guiActive = false;
            Events["EquipNearbyKerbal"].guiName = guiName;
        }

        [KSPEvent(guiActive = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1)]
        void EquipNearbyKerbal()
        {
            if (!FlightGlobals.ActiveVessel.isEVA || FlightGlobals.ActiveVessel.rootPart.Modules.Contains(moduleNode.GetValue("name")) || count == 0)
                return;
            else
            {
                FlightGlobals.ActiveVessel.rootPart.AddModule(moduleNode);
                FlightGlobals.ActiveVessel.rootPart.Modules[moduleNode.GetValue("name")].OnStart(PartModule.StartState.Flying);
                if (count > 0) count--;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode("MODULE"))
                moduleNode = node.GetNode("MODULE");
        }
    }
}
#endif