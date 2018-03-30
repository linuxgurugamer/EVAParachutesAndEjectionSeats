/*********************\
* ModuleKrAnimation   *
* (C) Kreuzung, CC BY *
\*********************/
#if false
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardTechnologies
{
    public class ModuleKrAnimation : PartModule
    {
        #region FieldsETC
        [KSPField(isPersistant = false)]
        public string extendAnim = "", extendGUI = "Extend", retractGUI = "Retract", controllableInside = "commandable", partManipulationConfigs = "", actionGUI = "Toggle", FXGroup = "";

        [KSPField(isPersistant = true)]
        public bool stage = false, state = false;

        [KSPField(isPersistant = false)]
        public float landingGear = 0, EVA_Range = 0, decoupleForce = -1, speed = 0, energyConsumptionExtend = 0, energyConsumptionRetract = 0, energyConsumptionExtended = 0;

        public List<VanguardTechnologies.PartManipulation> partManipulation = new List<VanguardTechnologies.PartManipulation>();

        protected Animation anim { get { return part.FindModelAnimators(extendAnim)[0]; } }
        protected AnimationState animState { get { return anim[extendAnim]; } }

        private bool applyManipulation = false;
        #endregion

        [KSPAction("Toggle")]
        public void Toggle(KSPActionParam param)
        {
            if (!IsContollable())
                return;
            if (!state && param.type == KSPActionType.Activate)
                ExtendAnimation();
            else if (state && param.type == KSPActionType.Deactivate)
                RetractAnimation();
        }

        [KSPEvent(guiActive = true, guiName = "Extend", guiActiveUnfocused = true, unfocusedRange = 0)]
        public void ExtendAnimation()
        {
            //retract does not like to find it in retracted state, so we don't extend if we can't move at all
            if (energyConsumptionExtend > 0 && part.RequestResource("ElectricCharge", energyConsumptionExtend / animState.length * TimeWarp.fixedDeltaTime) == 0) return;

            //foreach (AnimationState s in anim)
            //    s.speed = speed;
            animState.speed = speed;
            anim.Play(extendAnim);

            state = true;

            if (decoupleForce >= 0)
                part.decouple(decoupleForce);
        }

        [KSPEvent(guiActive = true, guiName = "Retract", guiActiveUnfocused = true, unfocusedRange = 0)]
        public void RetractAnimation()
        {
            //foreach (AnimationState s in anim)
            //    s.speed = -speed;
            animState.speed = -speed;
            anim.Play(extendAnim);

            //Otherwise the animation whould insta-jump to the retracted state
            //foreach (AnimationState s in anim)
            //    if (s.normalizedTime == 0)
            //        s.normalizedTime = 1;
            if (animState.normalizedTime == 0)
                animState.normalizedTime = 1;
            state = false;
        }

        public override void OnStart(PartModule.StartState state)
        {
            Log.Info("OnStart");

            Events["ExtendAnimation"].guiName = extendGUI;
            Events["RetractAnimation"].guiName = retractGUI;

            Events["ExtendAnimation"].unfocusedRange = EVA_Range;
            Events["RetractAnimation"].unfocusedRange = EVA_Range;

            Actions["Toggle"].guiName = actionGUI;

            if (speed == 0)
                speed = animState.speed;

            if (landingGear > 0)
                Actions["Toggle"].defaultActionGroup = KSPActionGroup.Gear;

            if (FXGroup != "")
                part.findFxGroup(FXGroup).Power = 1;

            if (this.state)
            {
                anim.Play(extendAnim);
                animState.normalizedTime = Convert.ToSingle(state);
            }

            Log.Info("loading part manipulation");

            string[] split = partManipulationConfigs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string file in split)
            {
                VanguardTechnologies.PartManipulation p = new VanguardTechnologies.PartManipulation();
                p.Load(file);
                partManipulation.Add(p);
            }
            applyManipulation = true;

            Log.Info("End of OnStart");
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight/* || vessel.packed*/) return;

            Events["ExtendAnimation"].active = IsContollable() && !state;
            Events["RetractAnimation"].active = IsContollable() && state && retractGUI != "none";

            #region staging
            if (stage && part.State == PartStates.ACTIVE)
            {
                stage = false;
                ExtendAnimation();
            }
            #endregion

            #region FX
            if (FXGroup != "")
                if (anim.isPlaying && !part.findFxGroup(FXGroup).Active)
                    part.findFxGroup(FXGroup).setActive(true);
                else if (!anim.isPlaying && part.findFxGroup(FXGroup).Active)
                    part.findFxGroup(FXGroup).setActive(false);
            #endregion
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight/* || vessel.packed*/) return;

            if (energyConsumptionExtended > 0 && state && !anim.isPlaying)
                if (part.RequestResource("ElectricCharge", energyConsumptionExtended * TimeWarp.fixedDeltaTime) == 0)
                    RetractAnimation();

            if (anim.isPlaying)
            {
                applyManipulation = true;
                foreach (VanguardTechnologies.PartManipulation p in partManipulation)
                    p.ApplyToPart(part, animState.normalizedTime);

                if (state && energyConsumptionExtend > 0)
                {
                    if (part.RequestResource("ElectricCharge", energyConsumptionExtend / animState.length * TimeWarp.fixedDeltaTime) == 0)
                        animState.enabled = false;
                }
                else
                    if (!state && energyConsumptionRetract > 0)
                    if (part.RequestResource("ElectricCharge", energyConsumptionRetract / animState.length * TimeWarp.fixedDeltaTime) == 0)
                        animState.enabled = false;
            }
            else if (applyManipulation)
            {
                applyManipulation = false;
                foreach (VanguardTechnologies.PartManipulation p in partManipulation)
                    p.ApplyToPart(part, Convert.ToSingle(state));
            }
        }

        public bool IsContollable()
        {
            switch (controllableInside)
            {
                case "never":
                    return !vessel.isActiveVessel;
                default:
                    if (!part.isControllable && vessel.isActiveVessel)
                        return false;
                    else
                        return true;
            }
        }
    }
}
#endif