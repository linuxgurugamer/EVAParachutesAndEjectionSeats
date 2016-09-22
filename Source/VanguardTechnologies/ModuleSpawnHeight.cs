using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if false
namespace VanguardTechnologies
{
    class ModuleKrSpawnHeight : PartModule
    {
        [KSPField]
        public float height = 0, setOrbit = 0;

        public void OnPutToGround(PartHeightQuery q)
        {
            Log.Info(q.lowestPoint.ToString());
            q.lowestPoint = -height;
            Log.Info(q.lowestPoint.ToString());
        }

        public override void OnStart(PartModule.StartState state)
        {
            if ((state & StartState.PreLaunch) != StartState.PreLaunch) return;
            if (setOrbit > 0)
                Invoke("SetOrbit", 1);
        }

        private void SetOrbit()
        {
            if (vessel.HoldPhysics)
            {
                Invoke("SetOrbit", 1);
                Log.Info("fail");
                return;
            }
            vessel.Landed = false;
            vessel.GoOnRails();
            double v = Math.Sqrt(vessel.mainBody.gravParameter / (vessel.mainBody.Radius + vessel.orbit.altitude));
            vessel.orbit.vel = Vector3.Cross(vessel.orbit.pos, new Vector3(0, 0, -1)).normalized * (float)v;
            vessel.GoOffRails();
            Log.Info("success");
        }
    }
}
#endif