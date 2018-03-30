using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if false
namespace VanguardTechnologies
{
    public class ModuleKrLightColor : PartModule
    {
        [KSPField(isPersistant = false)]
        public string lightName, emissiveName;

        private List<Light> lights;
        private List<Renderer> emissives;
        private ColorPickerWindow _win;
        private ColorPickerWindow win { get { return _win ?? (_win = ColorPickerWindow.CreateWindow("Light Colour", new Color(1,1,1,1))); } }

        public override void OnStart(PartModule.StartState state)
        {
            lights = part.FindModelComponents<Light>(lightName).ToList();
            Log.Info("[" + GetType().Name + "] Lights found: " + lights.Count);

            emissives = part.FindModelComponents<Renderer>(emissiveName).ToList();
            Log.Info("[" + GetType().Name + "] Emissives found: " + emissives.Count);
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                ConfigNode colourNode = node.GetNode("COLOUR") ?? node.GetNode("COLOR");
                if (colourNode == null) return;
                win.color = new Color(float.Parse(colourNode.GetValue("r")), float.Parse(colourNode.GetValue("g")), float.Parse(colourNode.GetValue("b")));
            }
            catch (Exception e)
            {
                win.color = new Color(1, 1, 1, 1);
                Debug.LogError("[" + GetType().Name + "] FAILED TO LOAD COLOUR");
                Log.Info(e.ToString());
            }
        }

        public override void OnSave(ConfigNode node)
        {
            ConfigNode colourNode = new ConfigNode("COLOR");
            colourNode.AddValue("r", win.color.r);
            colourNode.AddValue("g", win.color.g);
            colourNode.AddValue("b", win.color.b);
            node.AddNode(colourNode);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!win.IsVisible() && GameSettings.HEADLIGHT_TOGGLE.GetKey())
                {
                    RaycastHit r;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out r);
                    if (Part.FromGO(r.transform.gameObject) == part)
                        win.Show();
                }
            }
            lights.ForEach(l => l.color = win.color);
            Color emissiveClr = new Color(win.color.r, win.color.g, win.color.b);
            emissives.ForEach(e => e.material.SetColor("_EmissiveColor", emissiveClr));

        }

        [KSPEvent(guiActiveEditor = true, guiActive = true, guiName = "Show Colour Picker", guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 5)]
        public void ShowWindow() { win.Show(); }

        public void OnDestroy() { Destroy(_win); }
    }
}
#endif