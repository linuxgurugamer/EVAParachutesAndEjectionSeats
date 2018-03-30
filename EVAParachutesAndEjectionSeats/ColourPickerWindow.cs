using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if false
namespace VanguardTechnologies
{
    //Makes me want to do a kerbal anime hair mod
    public class ColorPickerWindow : FrementGUI.Window
    {
        public Color color;
        public bool showAlpha = false;
        private float f; private int i, selectedIndex;
        private string newPresetName = "";
        private static List<ConfigNode> presets;
        private Vector2 scrollPosition;

        public static ColorPickerWindow CreateWindow(string title, Color color = new Color(), bool showAlpha = false)
        {
            presets = new List<ConfigNode>();
            ConfigNode n = ConfigNode.Load(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/ColorPickerWindow.cfg") ?? new ConfigNode();
            foreach (ConfigNode c in n.nodes)
                presets.Add(c);

            ColorPickerWindow w = Cast<ColorPickerWindow>(FrementGUI.Window.CreateWindow(title));
            w.color = color;
            w.color.a = 1;
            w.showAlpha = showAlpha;
            return w;
        }

        public override void WindowElements()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("WARNING: You can click through this window!", HighLogic.Skin.label);
            //Red
            GUILayout.BeginHorizontal();
            GUILayout.Label("Red:", HighLogic.Skin.label, GUILayout.Width(50));
            if (float.TryParse(GUILayout.TextField(color.r.ToString(), HighLogic.Skin.textField), out f))
                color.r = Mathf.Clamp01(f);
            GUILayout.EndHorizontal();
            color.r = GUILayout.HorizontalSlider(color.r, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
            //Green
            GUILayout.BeginHorizontal();
            GUILayout.Label("Green:", HighLogic.Skin.label, GUILayout.Width(50));
            if (float.TryParse(GUILayout.TextField(color.g.ToString(), HighLogic.Skin.textField), out f))
                color.g = Mathf.Clamp01(f);
            GUILayout.EndHorizontal();
            color.g = GUILayout.HorizontalSlider(color.g, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
            //Blue
            GUILayout.BeginHorizontal();
            GUILayout.Label("Blue:", HighLogic.Skin.label, GUILayout.Width(50));
            if (float.TryParse(GUILayout.TextField(color.b.ToString(), HighLogic.Skin.textField), out f))
                color.b = Mathf.Clamp01(f);
            GUILayout.EndHorizontal();
            color.b = GUILayout.HorizontalSlider(color.b, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
            //alpha
            if (showAlpha)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Alpha:", HighLogic.Skin.label, GUILayout.Width(50));
                if (float.TryParse(GUILayout.TextField(color.a.ToString(), HighLogic.Skin.textField), out f))
                    color.a = Mathf.Clamp01(f);
                GUILayout.EndHorizontal();
                color.a = GUILayout.HorizontalSlider(color.a, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
            }
            //preset list
            if (presets.Count > 0)
            {
                selectedIndex = -1;
                for (int j = 0; j < presets.Count; j++)
                {
                    if (float.Parse(presets[j].GetValue("r")) == color.r && float.Parse(presets[j].GetValue("g")) == color.g
                        && float.Parse(presets[j].GetValue("b")) == color.b && float.Parse(presets[j].GetValue("a")) == color.a)
                    {
                        selectedIndex = j;
                        break;
                    }
                }
                GUILayout.Label("Select preset:", HighLogic.Skin.label);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, HighLogic.Skin.horizontalSlider, HighLogic.Skin.label, GUILayout.Height(48));
                i = GUILayout.Toolbar(selectedIndex, (from n in presets select n.GetValue("name")).ToArray(), HighLogic.Skin.button);
                GUILayout.EndScrollView();
                if (i >= 0)
                    color = new Color(float.Parse(presets[i].GetValue("r")), float.Parse(presets[i].GetValue("g")), float.Parse(presets[i].GetValue("b")), showAlpha ? float.Parse(presets[i].GetValue("a")) : 1);
            }
            else 
                selectedIndex = -1;

            //add preset
            newPresetName = GUILayout.TextField(newPresetName, HighLogic.Skin.textField);
            GUILayout.BeginHorizontal();
            if (selectedIndex == -1 && GUILayout.Button("Add preset", HighLogic.Skin.button))
            {
                ConfigNode n = new ConfigNode("PRESET");
                n.AddValue("r", color.r);
                n.AddValue("g", color.g);
                n.AddValue("b", color.b);
                n.AddValue("a", color.a);
                n.AddValue("name", newPresetName);
                presets.Add(n);
                SavePresets();
                newPresetName = "";
            }
            //rename preset
            if (selectedIndex >= 0 && GUILayout.Button("Rename preset", HighLogic.Skin.button))
            {
                presets[selectedIndex].SetValue("name", newPresetName);
                newPresetName = "";
                SavePresets();
            }
            //delete preset
            if (selectedIndex >= 0 && GUILayout.Button("Delete preset", HighLogic.Skin.button))
            {
                presets.RemoveAt(selectedIndex);
                SavePresets();
            }
            GUILayout.EndHorizontal();
            //End
            GUILayout.EndVertical();
        }

        private static void SavePresets()
        {
            if (presets.Count > 0)
            {
                ConfigNode parent = new ConfigNode("PRESETLIST");
                presets.ForEach(cn => parent.AddNode(cn));
                parent.Save(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/ColorPickerWindow.cfg");
            }
            else //config node fails parsing an empty file
                System.IO.File.Delete(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/ColorPickerWindow.cfg");
        }
    }
}
#endif