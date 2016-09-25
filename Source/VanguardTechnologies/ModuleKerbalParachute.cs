using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VanguardTechnologies
{
    public class ModuleKrKerbalParachute : PartModule
    {
        [KSPField(isPersistant = true)]
        public float deployedDrag = 100, closedDrag, minAirPressureToOpen = 0.01f, semiDeployedFraction = .25f, semiDeployedHeight = 1.25f, deployTime = .33f;

        public bool fullyDeployed = false;
        private GameObject chute;
        Vector3 targetSize, lastSize;
        float time;
        private bool deployWhenAble = false;
        int waitBeforeCheckingSrvVel = 0;
        int deployDelay = 0;
        double deployAfter = 0.0;
        public string chuteDir = "parachute";

        Vessel origVessel;
        float minVerticalSpeed = -1;

        string kerbalName;

        [KSPField(isPersistant = false, guiActive = true, guiName = "State")]
        public string chuteState;

        public override void OnStart(PartModule.StartState state)
        {
            Log.Info("ModuleKrKerbalParachute.OnStart, name:" + kerbalName);
            closedDrag = part.maximum_drag;
        }

        [KSPEvent(guiActive = true, guiName = "fully deploy parachute")]
        public void DeployFully()
        {
            if (chute && fullyDeployed) return;
            if (part.staticPressureAtm < minAirPressureToOpen)
            {
                Log.Info("Air pressure too low for EVA parachute");
                return;
            }
            Log.Info("EVA parachute fully deployed");
            CreateChuteModel();
            fullyDeployed = true;
            waitBeforeCheckingSrvVel = 5;
            targetSize = new Vector3(1, 1, 1);
        }

        [KSPEvent(guiActive = true, guiName = "semi-deploy parachute")]
        public void DeploySemi()
        {
            if (chute) return;
            if (part.staticPressureAtm < minAirPressureToOpen)
            {
                Log.Info("Air pressure too low for EVA parachute");
                return;
            }
            Log.Info("EVA parachute semi-deployed");
            CreateChuteModel();
            fullyDeployed = false;
            waitBeforeCheckingSrvVel = 5;
            targetSize = new Vector3(semiDeployedFraction, semiDeployedFraction, semiDeployedHeight);
        }

        public void DeployWhenAble(PartModule.StartState state, Vessel vessel, string k)
        {
            Log.Info("DeployWhenAble set");
            deployWhenAble = true;
            deployDelay = 2;
            origVessel = vessel;
            kerbalName = k;
            deployAfter = Planetarium.GetUniversalTime() + deployDelay;
            OnStart(state);
        }

        private void CreateChuteModel()
        {
            Log.Info("CreateChuteModel: " + chuteDir);
            time = (float)Planetarium.GetUniversalTime();
            if (!chute)
            {
                Log.Info("CreateChuteModel, Found, name: " + this.vessel.name);
                Log.Info("exists: " + GameDatabase.Instance.ExistsModel("VanguardTechnologies/Parts/" + chuteDir + "/model"));
                chute = GameDatabase.Instance.GetModel("VanguardTechnologies/Parts/" + chuteDir + "/model");
                chute.SetActive(true);
                chute.transform.parent = transform;//vessel.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/bn_jetpack01");
                chute.transform.localPosition = new Vector3(0, 0.1f, -0.2f); //new Vector3(0, 0.1f, 0);
                chute.transform.localScale = new Vector3(0, 0, 0);
            }
            lastSize = chute.transform.localScale;
        }


        //Log.Info("staticPressureAtm: " + part.staticPressureAtm.ToString() + "  minAirPressureToOpen: " + minAirPressureToOpen.ToString());
        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            // Once the vertical speed has been reached, set it to 0 so we don't keep the rest from working
            if (vessel.verticalSpeed > minVerticalSpeed)
                return;
            minVerticalSpeed = 0;
            if (deployAfter > Planetarium.GetUniversalTime())
                return;
            // Don't deploy if speed >150
            if (vessel.srf_velocity.magnitude > 150)
                return;

            Log.Info("FixedUpdate, name: " + this.vessel.name +
                "  deployWhenAble: " + deployWhenAble.ToString() + "  part.staticPressureAtm: " + part.staticPressureAtm.ToString() + "  minAirPressureToOpen" + minAirPressureToOpen.ToString());
            if (!chute)
                Log.Info("chute is null");
            else
                Log.Info("chute is good");
            if (!chute)
            {
                chuteState = part.staticPressureAtm < minAirPressureToOpen ? "Air pressure too low" : "Ready";
                if (chuteState == "Ready")
                {
                    if (deployWhenAble)
                    {
                        //DeployFully();
                        DeploySemi();
                        deployWhenAble = false;
                    }
                }
            }
            else
            {

                chuteState = fullyDeployed ? "fully deployed" : "semi-deployed";
            }

            if (!fullyDeployed && chute != null)
            {
                // if (GameSettings.EVA_Use.GetKey() && GameSettings.EVA_Jump.GetKey())
                if (vessel.altitude < 100 || (vessel.heightFromTerrain < 100 && vessel.heightFromTerrain != 1))
                    DeployFully();
                else DeploySemi();
            }
            if (chute)
            {
                float t = ((float)Planetarium.GetUniversalTime() - time) / deployTime;
                if (t < 1)
                    chute.transform.localScale = Vector3.Lerp(lastSize, targetSize, t);
                else
                    chute.transform.localScale = targetSize;

                part.maximum_drag = chute.transform.localScale.x * chute.transform.localScale.y * deployedDrag;
                chute.transform.LookAt(vessel.transform.position + vessel.srf_velocity);

                if (vessel.srf_velocity.sqrMagnitude < 0.1 && waitBeforeCheckingSrvVel == 0)
                {
                    part.maximum_drag = closedDrag;
                    Log.Info("EVA parachute closed, vessel.srf_velocity.sqrMagnitude: " + vessel.srf_velocity.sqrMagnitude.ToString());
                    Destroy(chute);
                }
                if (waitBeforeCheckingSrvVel > 0) waitBeforeCheckingSrvVel--;
            }
        }
    }
}