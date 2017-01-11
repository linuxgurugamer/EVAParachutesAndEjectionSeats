//This code was written by Frement, who allowed it to be freely used without any restrictions
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrementGUI
{
    public class OptionsWindow : Window
    {
        public float _background = 100.0f;
        public float _content = 100.0f;
        public float _control = 100.0f;

        public new static OptionsWindow CreateWindow()
        {
            OptionsWindow window = Cast<OptionsWindow>(Window.CreateWindow("Options", 300, 80, false));

            return window;
        }

        public override void WindowElements()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Background: ", new GUILayoutOption[] { GUILayout.Width(100.0f) });
            _background = GUILayout.HorizontalSlider(_background, 0.0f, 100.0f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Content: ", new GUILayoutOption[] { GUILayout.Width(100.0f) });
            _content = GUILayout.HorizontalSlider(_content, 0.0f, 100.0f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Control: ", new GUILayoutOption[] { GUILayout.Width(100.0f) });
            _control = GUILayout.HorizontalSlider(_control, 0.0f, 100.0f);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
