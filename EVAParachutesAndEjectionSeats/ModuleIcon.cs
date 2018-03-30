using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if false
namespace VanguardTechnologies
{
    public class ModuleKrIcon : PartModule
    {
        [KSPField]
        public string icon = "icons.png", grouping = "none";

        [KSPField] //Using float because int didn't work in .16, maybe it does in .17 though... fixing this up in .19.1 lol
        public int x = 0, y = 0;

        public override void OnStart(PartModule.StartState state)
        {
            part.stackIcon.SetIcon(icon, x, y);
            switch (grouping.ToLowerInvariant())
            {
                case "none":
                    part.stackIconGrouping = StackIconGrouping.NONE;
                    break;
                case "same_module":
                    part.stackIconGrouping = StackIconGrouping.SAME_MODULE;
                    break;
                case "same_hierarchy":
                    part.stackIconGrouping = StackIconGrouping.SAME_HIERARCHY;
                    break;
                case "same_type":
                    part.stackIconGrouping = StackIconGrouping.SAME_TYPE;
                    break;
                case "sym_counterparts":
                    part.stackIconGrouping = StackIconGrouping.SYM_COUNTERPARTS;
                    break;
            }
        }
        void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
                part.stackIcon.SetIcon(icon, x, y);
        }
    }
}
#endif