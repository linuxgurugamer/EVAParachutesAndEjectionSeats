using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VanguardTechnologies
{
    public class ModuleKrKerbalParachute : PartModule
    //    public class ModuleKrKerbalParachute : ModuleParachute
    {
        [KSPField(isPersistant = true)]
        public float deployedDrag = 100, closedDrag, minAirPressureToOpen = 0.01f, semiDeployedFraction = .25f, semiDeployedHeight = 1.25f, deployTime = .33f;

        public bool fullyDeployed = false;
        public bool deployed = false;
        private GameObject chute;
        Vector3 targetSize, lastSize;
        float time;
        private bool deployWhenAble = false;
        int waitBeforeCheckingSrvVel = 0;
        int deployDelay = 0;
        double deployAfter = 0.0;
        public string chuteDir = "parachute";
        public bool parasail = false;
        //public Rigidbody rigidbody;

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
            deployed = true;
            waitBeforeCheckingSrvVel = 30;
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
            deployed = true;
            waitBeforeCheckingSrvVel = 500;
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
                // this.rigidbody = GetComponent<Rigidbody>();
                //this.GetComponentCached(ref this.rigidbody);

                chute.transform.parent = transform;//vessel.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/bn_jetpack01");
                chute.transform.localPosition = new Vector3(0, 0.1f, -0.2f); //new Vector3(0, 0.1f, 0);
                chute.transform.localScale = new Vector3(0, 0, 0);

            }
            lastSize = chute.transform.localScale;
        }

        //  bool i = false;
        //  double degree = 0;
        //Log.Info("staticPressureAtm: " + part.staticPressureAtm.ToString() + "  minAirPressureToOpen: " + minAirPressureToOpen.ToString());
        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !deployed) return;

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

            if (!chute && GameSettings.EVA_Use.GetKey() && GameSettings.EVA_Jump.GetKey())
            {
                Log.Info("EVA_Use & EVA_Jump");
                DeploySemi();
            }
            if (!fullyDeployed && chute != null)
            {
                if (vessel.altitude < 200 || (vessel.heightFromTerrain < 200 && vessel.heightFromTerrain != 1))
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

                //chute.transform.localScale = new Vector3(0.55f, 0.55f, semiDeployedHeight); // test
                
                Log.Info("part.mass: " + part.mass.ToString() + "   part.physicsMass:  " + part.physicsMass.ToString());
                part.maximum_drag = chute.transform.localScale.x * chute.transform.localScale.y * deployedDrag;
                //                part.minimum_drag = part.maximum_drag;

                //                fullyDeployedDrag = 100;
                //                areaDeployed = 100;
                //                Deploy();
                //                deploymentState = deploymentStates.DEPLOYED;

                
                Vector3 skywardsDirection =  -1 *vessel.srf_velocity;

                float force = (float)(FlightGlobals.ActiveVessel.mainBody.GeeASL * 9.81 * part.physicsMass * chute.transform.localScale.x);
                // Following to smooth the descent speed
                if (chute.transform.localScale.x == 1)
                {
                    if (vessel.srf_velocity.magnitude > 20)
                        force *= 2;
                    if (vessel.srf_velocity.magnitude > 5 && vessel.srf_velocity.magnitude <= 20)
                        force = Mathf.Lerp(force * 2, force, (float)vessel.srf_velocity.magnitude / 15);
                    if (vessel.srf_velocity.magnitude < 5)
                        force -= 0.1f;
                }
                Log.Info("force: " + force.ToString() + "  FlightGlobals.ActiveVessel.mainBody.GeeASL: " + FlightGlobals.ActiveVessel.mainBody.GeeASL.ToString() + "  part.physicsMass: " + part.physicsMass.ToString() + "  chute.transform.localScale.x: " + chute.transform.localScale.x.ToString());
                vessel.rootPart.Rigidbody.AddForce(skywardsDirection.normalized * force );

                Log.Info("maximum_drag: " + part.maximum_drag.ToString() + "   deployedDrag: " + deployedDrag.ToString());
                Log.Info("parasail: " + parasail.ToString());
                Log.Info("skywardsDirection: " + skywardsDirection.normalized.ToString());
#if true
                // Vector3 velocity = vessel.rootPart.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
                //Log.Info("velocity: " + velocity.ToString());
                //Log.Info("deployedDrag: " + deployedDrag.ToString());
                // velocity = this.part.Rigidbody.velocity ;
                //this.sqrSpeed = velocity.sqrMagnitude;
                //Vector3 dragVector = -velocity.normalized;

                if (parasail)
                {
                    //if (!i && t > 1)
                    //{
                       // chute.transform.Rotate(180 - FlightGlobals.ship_heading, 90, 0, Space.World);
                    //    i = true;
                    //}
                    //degree += 0.5;
                    //Log.Info("degree: " + degree.ToString());
                    //chute.transform.Rotate(0, 0, 0, Space.World);

                    Vector3 dragVector = vessel.transform.forward;
                    Log.Info("dragVector: " + dragVector.ToString());
                    Vector3 dragForce = deployedDrag * dragVector * semiDeployedFraction;
                    vessel.rootPart.Rigidbody.AddForceAtPosition(dragForce / 4, this.part.transform.position);

                    Log.Info("x chute.transform.rotation : " + chute.transform.rotation.ToString());
                    Log.Info("vessel.transform.rotation : " + vessel.transform.rotation.ToString());
                    Log.Info("vessel.transform.rotation.x : " + vessel.transform.rotation.x.ToString());
                    //Quaternion tr = chute.transform.rotation;
                    //tr.y = chute.transform.rotation.y * 0.9f;
                    //chute.transform.rotation = tr;
                    Log.Info("chute.transform.rotation.y : " + chute.transform.rotation.y.ToString());
                    //chute.transform.RotateAround(chute.transform.position, chute.transform.up, 1);
                    //chute.transform.Rotate(0, 1, 0, Space.Self);

                    //Vector3 fromPosition = chute.transform.position;
                    //Quaternion fromRotation = chute.transform.rotation;

                    //  chute.transform.position = Vector3.Lerp(fromPosition, vessel.rootPart.transform.position, 0);
                    //chute.transform.rotation = vessel.rootPart.transform.rotation;

                    //chute.transform.localRotation = vessel.rootPart.transform.localRotation;
                    // chute.transform.parent = transform;
                    // chute.transform.localPosition = new Vector3(0, 0.1f, -0.2f);
                    //chute.transform.eulerAngles = new Vector3(chute.transform.eulerAngles.x, vessel.rootPart.transform.eulerAngles.y, chute.transform.eulerAngles.z);
                //    Quaternion target = Quaternion.identity;

                 //   target = Quaternion.LookRotation(this.vessel.srf_velocity);
                //    Vector3d relativeTargetFacing = Quaternion.Inverse(this.vessel.transform.rotation)  * Vector3d.forward;

                    // target = Quaternion.AngleAxis(hdgAngle, target * Vector3.up) * target; // heading rotation
                    // target = Quaternion.AngleAxis(pitchAngle, target * -Vector3.right) * target; // pitch rotation

                    //chute.transform.localRotation = target;
                  //  chute.transform.rotation = Quaternion.Euler(relativeTargetFacing);
                }
#endif
                //var tt = vessel.transform.TransformDirection(Vector3.forward);

                Log.Info("Vessel pointing: " + FlightGlobals.ship_heading.ToString());
                Log.Info("vessel.srf_velocity: " + vessel.srf_velocity.ToString());
                Log.Info("vessel.srf_velocity.x: " + vessel.srf_velocity.x.ToString());
                Log.Info("vessel.srf_velocity.y: " + vessel.srf_velocity.y.ToString());
                Log.Info("vessel.srf_velocity.z: " + vessel.srf_velocity.z.ToString());


                if (parasail)
                {
                    var s = Vector3.left;
                    s.x = 0.1f;
                    s = -1 * s;
                    //s.x = 0;
                    //s.z = 0;


                    chute.transform.LookAt(chute.transform.position + s);

                    //vessel.rootPart.Rigidbody.AddForceAtPosition(part.maximum_drag  * (chute.transform.position - s), this.part.transform.position);
                    //chute.transform.RotateAround(chute.transform.position, chute.transform.up, FlightGlobals.ship_heading);
                    //                chute.transform.LookAt(vessel.transform.position + s);
                    //  chute.transform.Rotate(0, 0.1f, 0, Space.World);
                }
                else
                {
                    chute.transform.LookAt(vessel.transform.position + vessel.srf_velocity);
                }

#if false
                if (parasail)
                {
                    Quaternion tr2 = chute.transform.localRotation;
                    tr2.z = chute.transform.localRotation.z * FlightGlobals.ship_heading/360;

                    
                    Vector3d target = Quaternion.AngleAxis(1, Vector3d.up);
                    chute.transform.localRotation = chute.transform.localRotation + target;
                }
#endif

                if (vessel.srf_velocity.sqrMagnitude < 0.1 && waitBeforeCheckingSrvVel == 0)
                {
                    part.maximum_drag = closedDrag;
                    Log.Info("EVA parachute closed, vessel.srf_velocity.sqrMagnitude: " + vessel.srf_velocity.sqrMagnitude.ToString());
                    Destroy(chute);
                }

                if (waitBeforeCheckingSrvVel > 0) waitBeforeCheckingSrvVel--;
            }
        }

#if false
        void t()
        {
            // Create a triangle and give it a shape
            GameObject obj3 = new GameObject("Line");
            greenline = obj3.AddComponent<LineRenderer>();
            greenline.transform.parent = transform;       //double check that 'transform' is in fact the Kerbal's transform. It seems to be based on your next posts, but the code itself is not clear.
            greenline.useWorldSpace = true; // true = Stay static on the ground. false = Move with part [b][COLOR="#FF0000"]Worldspace is needed as the line we are drawing will be relative to two separate objects[/COLOR][/b]
            greenline.material = new Material(Shader.Find("Particles/Additive"));
            greenline.SetColors(Color.green, Color.green); // Make it green
            greenline.SetWidth(0, 2); // Make it width 0 at point 0, width 2 at point 1
            greenline.SetVertexCount(2); // Haven't toyed with this yet [b][COLOR="#FF0000"]this is the number of Vertex's on the line. A straight line will always have 2[/COLOR][/b]

            // Place triangle in the world relative to part that created it (a Kerbal)
            
            Vector3 skywardsDirection = Kerbal.body.position - Kerbal.transform.position; //get our vector from the center of the planet we are standing on to the center of our kerbal. Refresh this every frame since it changes when the kerbal moves[/COLOR][/b]
            greenline.SetPosition(0, Vector3.zero); // Point 0 is located at Kerbal's location, Vector3.zero 
            greenline.SetPosition(1, 5 * skywardsDirection.normalize); //Point 1 is located 5 meters away in the direction calculated from the center of the planet to our kerbal. Vector3.normalize takes any vector and makes it 1 meter long.[/COLOR][/b]

        }
#endif

    }
}