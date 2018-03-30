using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VanguardTechnologies
{
    public class ModuleKrPartInfo : PartModule
    {
        bool alreadyPrintedInternal = false;
        public override void OnStart(PartModule.StartState state)
        {
            Log.Info("--- PART INFO ---");

            Log.Info("fxgroups:");

            foreach (FXGroup f in part.fxGroups)
                Log.Info(f.name);

            Log.Info("Attach nodes:");

            foreach (AttachNode n in part.attachNodes)
                Log.Info(n.id);
            Log.Info("--- MODEL INFO ---");

            Log.Info("Animations:");
            foreach (Animation a in part.FindModelAnimators())
            {

                Log.Info("* " + a.name);

                foreach (AnimationState s in a)
                    Log.Info("** " + s.name);
            }

            Log.Info("Transforms:");

            printTransforms(part.transform);


            Log.Info("--- END OF MODEL INFO ---");
        }

        public override void OnUpdate()
        {
            if (part.internalModel != null && !alreadyPrintedInternal)
            {
                Log.Info("--- INTERNAL INFO ---");
                Log.Info("Internal transforms:");

                printTransforms(part.internalModel.transform);
                alreadyPrintedInternal = true;
                Log.Info("--- END OF INTERNAL INFO ---");
            }
        }
        public static void printTransforms(Transform t, string prefix = "")
        {
            Log.Info(prefix + t.name);
            prefix += "*";
            for (int i = 0; i < t.childCount; i++)
                printTransforms(t.GetChild(i), prefix);
        }
    }
}
