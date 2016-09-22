//This code was written by Frement, who allowed it to be freely used without any restrictions
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrementGUI
{
    public class OverlayWindow : Window
    {
        private string message;
        private GUIStyle labelStyle;

        public static OverlayWindow CreateWindow(string message)
        {
            OverlayWindow window = Cast<OverlayWindow>(Window.CreateWindow("", Screen.width + 200, Screen.height + 200, -100.0f, -100.0f, false));
            window.message = message;
            window.labelStyle = new GUIStyle();
            window.labelStyle.alignment = TextAnchor.MiddleCenter;
            window.labelStyle.normal.textColor = Color.white;
            window.labelStyle.focused.textColor = Color.white;
            window.labelStyle.active.textColor = Color.white;
            window.labelStyle.hover.textColor = Color.white;

            return window;
        }

        public void SetMessage(string message)
        {
            this.message = message;
        }

        public override void WindowElements()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUIContent msg = new GUIContent(message);
            Vector2 labelSize = labelStyle.CalcSize(msg);
            GUI.Label(new Rect(((Screen.width + 200.0f) / 2) - (labelSize.x / 2), ((Screen.height + 200.0f) / 2) - (labelSize.y / 2), 500.0f, 50.0f), msg);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            Event.current.Use();
        }
    }
}
