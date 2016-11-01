using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VanguardTechnologies
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DoEjections : MonoBehaviour
    {
        public static DoEjections Instance;
        double lastTime = 0.0;
        Vessel vessel = null;
        const float DELAY = 3.0f;

        bool allSpawned = true;
        Vessel ejectedKerbal = null;
        float ejectionForce = 1000;
        int forceCnt = 60;
        float distance;
        Vessel origVessel;



        public string ejectionSoundFile = "VanguardTechnologies/Sounds/ejectionSound";

        public float ejectionVolume = 1f;

        public FXGroup EjectionSound = null;
        private float soundPitch = 1;
        private float soundVolume = 0;



        void Start()
        {
            Instance = this;

            {
                EjectionSound = new FXGroup("ejectionSound");

                if (EjectionSound != null)
                {

                    EjectionSound.audio = this.gameObject.AddComponent<AudioSource>();
                    EjectionSound.audio.dopplerLevel = 0f;
                    EjectionSound.audio.Stop();

                    EjectionSound.audio.loop = false;

                    //EjectionSound.audio.time = EjectionSound.audio.clip.length;
                }
                else
                    Debug.LogError("DoEjections: Sound FXGroup not found.");

            }
        }

        public void SetVessel(Vessel v)
        {
            Log.Info("vessel abort: " + v.name);
            vessel = v;
            lastTime = 0.0;
        }

        bool FindEjector(Part p, out ModuleKrEjectPilot mkep)
        {
            mkep = null;
            foreach (PartModule m in p.Modules)
            {
                if (m.moduleName == "ModuleKrEjectPilot")
                {
                    mkep = (ModuleKrEjectPilot)m;
                    Log.Info("EjectorFound on command pod");
                    if (mkep.maxUses > 0)
                    {
                        //ejectorFound = true;
                        mkep.maxUses--;
                        ejectionForce = mkep.ejectionForce;
                        forceCnt = 60;
                        Log.Info("Max uses: " + mkep.maxUses.ToString() + "   EjectionForce: " + ejectionForce.ToString());
                        return true;
                    }

                }
            }
            return false;
        }

        void Update()
        {
            if (vessel == null)
                return;
            double d = Planetarium.GetUniversalTime();
            if (d < lastTime + DELAY)
                return;

            lastTime = d;
            origVessel = vessel;
            ModuleKrEjectPilot mkep = null;
            foreach (Part p in vessel.parts)
            {
                if (p.protoModuleCrew.Count == 0)
                {
                    Log.Info("nobody inside");
                    continue;
                }
                bool ejectorFound = false;

                foreach (Part childp in p.FindChildParts<Part>(false))
                {
                    ejectorFound = FindEjector(childp, out mkep);
                    
#if false
                    foreach (PartModule m in childp.Modules)
                    {
                        if (m.moduleName == "ModuleKrEjectPilot")
                        {
                            mkep = (ModuleKrEjectPilot)m;
                            Log.Info("EjectorFound on command pod");
                            if (mkep.maxUses > 0)
                            {
                                ejectorFound = true;
                                mkep.maxUses--;
                                ejectionForce = mkep.ejectionForce;
                                forceCnt = 60;
                                Log.Info("Max uses: " + mkep.maxUses.ToString() + "   EjectionForce: " + ejectionForce.ToString());
                                break;
                            }

                        }
                    }
#endif
                    if (ejectorFound)
                        break;
                }
                // This is for the case where the ejection module has been added to the command pod directly
                if (!ejectorFound)
                {
                    // If an ejection part IS mounted, but is usedup, mkep will not be null
                    // This will give the ability to override a built-in ejection module
                    if (mkep == null)
                        ejectorFound = FindEjector(p, out mkep);
#if false
                    foreach (PartModule m in p.Modules)
                    {
                        if (m.moduleName == "ModuleKrEjectPilot")
                        {
                            mkep = (ModuleKrEjectPilot)m;
                            Log.Info("EjectorFound on command pod");
                            if (mkep.maxUses > 0)
                            {
                                ejectorFound = true;
                                mkep.maxUses--;
                                ejectionForce = mkep.ejectionForce;
                                forceCnt = 60;
                                Log.Info("Max uses: " + mkep.maxUses.ToString() + "   EjectionForce: " + ejectionForce.ToString());
                                break;
                            }

                        }
                    }
#endif
                    if (!ejectorFound)
                    {
                        Log.Info("EjectorNot found on command pod");
                        continue;
                    }
                }

                // Look through all the available crew until we find one which can be ejected

                foreach (ProtoCrewMember kerbal in p.protoModuleCrew)
                {
                    KerbalEVA spawned = FlightEVA.fetch.spawnEVA(kerbal, p, p.airlock, true);
                    if (!spawned)
                        // if false, then the exit was blocked by something
                        allSpawned = false;
                    else
                    {
                        spawned.autoGrabLadderOnStart = false;

                        // Look for the kerbal in all the vessels so we can 
                        // add the parachute module and mark it as deployable
                        for (int i = FlightGlobals.Vessels.Count - 1; i >= 0; i--)
                        {
                            if (kerbal.name == FlightGlobals.Vessels[i].vesselName)
                            {
                                Log.Info("DoEjections.Update, adding parachute to " + kerbal.name);
                                bool b = true;
                                foreach (Part prt in FlightGlobals.Vessels[i].parts)
                                {
                                    // Check to see if this kerbal already has a parachute module, if it doesn't
                                    // then add it
                                    foreach (PartModule m in prt.Modules)
                                    {
                                        if (m.moduleName == "ModuleKrKerbalParachute")
                                        {
                                            b = false;
                                            break;
                                        }
                                    }
                                }
                                if (b)
                                    FlightGlobals.Vessels[i].rootPart.AddModule("ModuleKrKerbalParachute");

                                ModuleKrKerbalParachute mkkp = (ModuleKrKerbalParachute)FlightGlobals.Vessels[i].rootPart.Modules["ModuleKrKerbalParachute"];
                                mkkp.deployedDrag = mkep.deployedDrag;
                                mkkp.minAirPressureToOpen = mkep.minAirPressureToOpen;
                                mkkp.semiDeployedFraction = mkep.semiDeployedFraction;
                                mkkp.deployTime = mkep.deployTime;
                                mkkp.deployed = true;

                                //mkkp.rigidbody = this.

                                Log.Info("mkep.selectedChute: " + mkep.selectedChute.ToString());
                                if (mkep.selectedChute >= 0 && mkep.selectedChute < mkep.arrChuteDir.Length)
                                    mkkp.chuteDir = mkep.arrChuteDir[mkep.selectedChute];

                                if (mkep.arrChuteDir[mkep.selectedChute] == "parasailChute")
                                    mkkp.parasail = true;

                                //Log.Info("mkkp.deployedDrag: " + mkkp.deployedDrag.ToString() + "   mkep.deployedDrag: " + mkep.deployedDrag.ToString());
                                //Log.Info("mkkp.minAirPressureToOpen: " + mkkp.minAirPressureToOpen.ToString() + "   mkep.minAirPressureToOpen: " + mkep.minAirPressureToOpen.ToString());
                                //Log.Info("mkkp.semiDeployedFraction: " + mkkp.semiDeployedFraction.ToString() + "   mkep.semiDeployedFraction: " + mkep.semiDeployedFraction.ToString());
                                //Log.Info("mkkp.deployTime: " + mkkp.deployTime.ToString() + "   mkep.deployTime: " + mkep.deployTime.ToString());

                                mkkp.DeployWhenAble(PartModule.StartState.Flying, origVessel, FlightGlobals.Vessels[i].rootPart.name);
                                ejectedKerbal = FlightGlobals.Vessels[i];
                                distance = Vector3.Distance(ejectedKerbal.rootPart.transform.position, origVessel.rootPart.transform.position);

                                if (EjectionSound != null)
                                {
                                    if (!GameDatabase.Instance.ExistsAudioClip(ejectionSoundFile))
                                    {
                                        Debug.LogError("DoEjections: Audio file not found: " + ejectionSoundFile);

                                    }
                                    else
                                    {
                                        EjectionSound.audio.clip = GameDatabase.Instance.GetAudioClip(ejectionSoundFile);
                                        soundVolume = GameSettings.SHIP_VOLUME * ejectionVolume;
                                        //soundPitch = Mathf.Lerp(0.5f, 1f, 1f);
                                        soundPitch = 1f;
                                        EjectionSound.audio.pitch = soundPitch;
                                        EjectionSound.audio.volume = mkep.Volume;
                                        //RcsSound.audio.loop = true;

                                        EjectionSound.audio.PlayOneShot(EjectionSound.audio.clip);
                                        // if (!RcsSound.audio.isPlaying)
                                        //RcsSound.audio.Play();
                                    }

                                }

                                return;
                            }
                        }
                    }
                }

            }
            if (allSpawned)
                vessel = null;
        }

        void FixedUpdate()
        {

            if (ejectedKerbal != null)
            {
                Log.Info("DoEjections.FixedUpdate");
                KerbalEVA kEVA = ejectedKerbal.GetComponentInChildren<KerbalEVA>();
                if (kEVA != null)
                {
                    Log.Info("KerbalEVA Found, name: " + this.vessel.name);
                    if (kEVA.Ready)
                    {
#if true
                        Vector3 direction = ejectedKerbal.rootPart.transform.position - origVessel.rootPart.transform.position;
                        direction.Normalize();

                        if (ejectionForce > 0)
                            ejectedKerbal.rootPart.AddForce(direction * ejectionForce);
                        if (forceCnt-- < 0)
                            ejectionForce = 0;
                        // RcsSound.audio.loop = false;
                        if (distance * 2 < Vector3.Distance(ejectedKerbal.rootPart.transform.position, origVessel.rootPart.transform.position))
                            ejectedKerbal = null;
#else
                        // this is an alternative way to force the Kerbal to let go of the ladder
                        if (kEVA.OnALadder && kEVA.Ready)
                        {
                            // Following from xEvilReeperX                       
                            var fsm = FlightGlobals.ActiveVessel.gameObject.GetComponent<KerbalEVA>().fsm;
                            var letGoEvent = fsm.CurrentState.StateEvents.SingleOrDefault(e => e.name == "Ladder Let Go");

                            if (letGoEvent == null)
                                Debug.LogError("Did not find let go event");
                            else fsm.RunEvent(letGoEvent);


                        }
#endif

                    }
                }
            }
        }
    }

    public class ModuleKrEjectPilot : PartModule, IPartMassModifier, IPartCostModifier
    {

        [KSPField]
        public int maxUses = 3;

        [KSPField(guiActiveEditor = true, guiName = "Min Pressure", isPersistant = true),
            UI_FloatRange(minValue = 0.01f, maxValue = 1f, stepIncrement = 0.01f)]
        public float minAirPressureToOpen = 0.04f;

        //mass
        [KSPField]
        public float baseMass = 0.05f;

        [KSPField]
        public int baseCost = 666;

        [KSPField]
        public float ejectionForce = 1000;

        [KSPField]
        public float deployedDrag = 100f;
        [KSPField]
        public float closedDrag = 0.0f;

        [KSPField]
        public float semiDeployedFraction = .25f;
        [KSPField]
        public float semiDeployedHeight = 1.25f;
        [KSPField]
        public float deployTime = .33f;

        //   [KSPField]
        //  public string SoundFile = "VanguardTechnologies/Sounds/ejectionSound";

        [KSPField]
        public float Volume = 1f;



#if true
        [KSPField(isPersistant = true)]
        public int selectedChute = 0;

        [KSPField(guiActiveEditor = true, guiActive = false, guiName = "ChuteSwitcher", isPersistant = true)]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.Editor, scene = UI_Scene.All, suppressEditorShipModified = false, display = new string[] { "None" })]
        public string ChooseOption = "0";
        private string[] Chutes;
        BaseField chooseChute;
#endif

        //private ProtoCrewMember kerbal;
        private bool ejecting = false;

        [KSPAction("Eject Crew", KSPActionGroup.Abort)]
        public void Eject(KSPActionParam param)
        {
            ejecting = true; // param.type == KSPActionType.Activate;
            part.SendEvent("OnDeboardSeat");
            Log.Info("Eject Crew");
        }


        string[] arrChuteNames = new string[7] { "Round", "Round 2", "Round 3", "Parasail", "Square", "Square 3", "Square 4" };
        public string[] arrChuteDir = new string[7] { "roundChute", "roundChute2", "roundChute3", "parasailChute", "squareChute", "squareChute3", "squareChute4" };


        void Start()
        {
            SetupGUI();
        }

        private void SetupGUI()
        {
            Log.Info("SetupGUI");
            //Update the gui
            chooseChute = Fields[nameof(ChooseOption)];
            chooseChute.guiName = "Chute";     //Dummy name until updated
            chooseChute.guiActiveEditor = true;
            chooseChute.guiActive = false;

            //Create array Options that are simple ref's to the propellant list
            Chutes = new string[arrChuteNames.Length];
            for (int i = 0; i < arrChuteNames.Length; i++)
            {
                Chutes[i] = i.ToString();
            }

            //Set which function run's when changing selection, which options, and the text to display
            //var chooseOption = chooseField.uiControlEditor as UI_ChooseOption;
            UI_ChooseOption chooseOption = chooseChute.uiControlEditor as UI_ChooseOption;
            chooseOption.options = Chutes;
            chooseOption.display = arrChuteNames;        //Should be GUInames array
            chooseOption.onFieldChanged = selectChute;
        }
        //onFieldChanged action
        private void selectChute(BaseField field, object oldValueObj)
        {
            Log.Info("selectChute");
            selectedChute = int.Parse(ChooseOption);
            Log.Info("selectedChute: " + selectedChute.ToString());
            // updateIntake(true);
        }


        int getNumSeats()
        {
            int cnt;
            Log.Info("getNumSeats");
            if (HighLogic.LoadedSceneIsEditor && this.part.parent == null)
            {
                cnt = maxUses;
            }
            else
            {
                if (this.part.parent == null)
                    cnt = System.Math.Min(this.part.CrewCapacity, maxUses);
                else
                    cnt = System.Math.Min(this.part.parent.CrewCapacity, maxUses);
            }
            //Log.Info("Ejector Cnt: " + cnt.ToString());
            return cnt;
#if false
            int cnt = 0;
            if (HighLogic.LoadedSceneIsEditor)
            {
                //                shipSize = ShipConstruction.findFirstCrewablePart(EditorLogic.fetch.ship);
                //                EditorLogic.fetch.ship.parts

                foreach (var part in EditorLogic.fetch.ship.parts.Where(part => part.CrewCapacity > 0).ToList())
                    cnt += part.CrewCapacity;
                // Log.Info("Editor Vessel crew capacity: " + cnt.ToString());
                return cnt;
            }
            else
            {
                if (FlightGlobals.ActiveVessel)
                    foreach (var part in FlightGlobals.ActiveVessel.parts.Where(part => part.CrewCapacity > 0).ToList())
                        cnt += part.CrewCapacity;
                // Log.Info("Flight Vessel crew capacity: " + cnt.ToString());
                return cnt;

#endif
        }
        

        public float GetModuleMass(float defaultMass, ModifierStagingSituation mss)
        {
            if (this.part.parent == null)
                defaultMass = baseMass;
            //Log.Info("GetModuleMass   defaultMass: " + defaultMass.ToString() + "   baseMass: " + baseMass.ToString() + "  numSeats: " + getNumSeats().ToString());
            return (defaultMass + baseMass * getNumSeats());
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation mss)
        {
            return (defaultCost + baseCost * getNumSeats());
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (ejecting)
            {
                if (maxUses < 0)
                {
                    part.explode();
                }

                DoEjections.Instance.SetVessel(vessel);

                ejecting = false;
            }
        }

        public override string GetInfo()
        {
            string desc = "Has ejection capability";
            if (maxUses > 0) desc += " for " + maxUses + " crew members";
            return desc;
        }
    }
}