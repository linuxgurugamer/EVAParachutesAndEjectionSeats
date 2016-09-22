//This code was written by Frement, who allowed it to be freely used without any restrictions
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrementGUI
{
    public class Window : MonoBehaviour
    {
        //Kreuzung - Added parent object directly to window class
        private static GameObject _parentObject;
        private static GameObject parentObject { get { if (!_parentObject) { _parentObject = new GameObject("FrementGUIObject"); DontDestroyOnLoad(_parentObject); } return _parentObject; } }

        private Rect position;
        private string title = "Default";
        private int width, height;
        private GUIStyle style = new GUIStyle(HighLogic.Skin.window);
        private int id = 19856745;
        private static System.Random random = new System.Random();
        private bool show = false;
        private bool controls = true;
        private bool _minimizeWindow = false;
        private int _minimizeHeight = 60;
        private static Texture2D _optionsTex = null;
        private static Texture2D _minimizeTex = null;
        private static Texture2D _closeTex = null;
        private static Texture2D _optionsHoverTex = null;
        private static Texture2D _minimizeHoverTex = null;
        private static Texture2D _closeHoverTex = null;
        private bool _options = false;
        private bool _minimize = false;
        private bool _close = false;
        private static GUIStyle _optionsStyle = new GUIStyle();
        private static GUIStyle _minimizeStyle = new GUIStyle();
        private static GUIStyle _closeStyle = new GUIStyle();
        private OptionsWindow optionsWindow = null;

        public static T Cast<T>(Window src) where T : Window, new()
        {
            T dest = parentObject.AddComponent<T>();
            dest.id = src.id;
            dest.controls = src.controls;
            dest.width = src.width;
            dest.height = src.height;
            dest.optionsWindow = src.optionsWindow;
            dest.position = src.position;
            dest.title = src.title;
            dest.style = src.style;
            dest.show = src.show;

            return dest;
        }

        public static Window CreateWindow()
        {
            Window window = parentObject.AddComponent<Window>();
            window.id = random.Next(1000000, 9000000);

            if (Window._optionsTex == null)
                Window._optionsTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/options", false);
            if (Window._minimizeTex == null)
                Window._minimizeTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/minimize", false);
            if (Window._closeTex == null)
                Window._closeTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/close", false);
            if (Window._optionsHoverTex == null)
                Window._optionsHoverTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/options_hover", false);
            if (Window._minimizeHoverTex == null)
                Window._minimizeHoverTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/minimize_hover", false);
            if (Window._closeHoverTex == null)
                Window._closeHoverTex = GameDatabase.Instance.GetTexture("VNG/FrementGUI/close_hover", false);

            Window._optionsStyle.hover.background = Window._optionsHoverTex;
            Window._minimizeStyle.hover.background = Window._minimizeHoverTex;
            Window._closeStyle.hover.background = Window._closeHoverTex;

            Window._optionsStyle.normal.background = Window._optionsTex;
            Window._minimizeStyle.normal.background = Window._minimizeTex;
            Window._closeStyle.normal.background = Window._closeTex;

            return window;
        }

        public static Window CreateWindow(string title, bool controls = true)
        {
            Window window = CreateWindow();
            window.controls = controls;
            window.width = 300;
            window.height = 150;
            Vector2 centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            window.position = new Rect(centerPoint.x - (window.width / 2), centerPoint.y - (window.height / 2), window.width, window.height);
            window.title = title;
            if (controls)
            {
                window.optionsWindow = OptionsWindow.CreateWindow();
            }

            return window;
        }

        public static Window CreateWindow(string title, int width, int height, bool controls = true)
        {
            Window window = CreateWindow();
            window.controls = controls;
            window.width = width;
            window.height = height;
            Vector2 centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            window.position = new Rect(centerPoint.x - (window.width / 2), centerPoint.y - (window.height / 2), window.width, window.height);
            window.title = title;
            if (controls)
            {
                window.optionsWindow = OptionsWindow.CreateWindow();
            }

            return window;
        }

        public static Window CreateWindow(string title, int width, int height, float left, float top, bool controls = true)
        {
            Window window = CreateWindow();
            window.controls = controls;
            window.width = width;
            window.height = height;
            window.position = new Rect(left, top, window.width, window.height);
            window.title = title;
            if (controls)
            {
                window.optionsWindow = OptionsWindow.CreateWindow();
            }

            return window;
        }

        public void SetSize(int width, int height, bool save = true)
        {
            if (save)
            {
                this.width = width;
                this.height = height;
            }
            position = new Rect(position.xMin, position.yMin, width, height);
        }

        public bool IsVisible()
        {
            return show;
        }

        public void Show()
        {
            show = true;
        }

        public void Hide()
        {
            show = false;
            if (controls)
                optionsWindow.Hide();
        }

        public void Toggle()
        {
            show = !show;
            if (!show && controls)
                optionsWindow.Hide();
        }

        private void OnGUI()
        {
            if (show)
            {
                Color color = GUI.color;
                if (controls)
                    GUI.color = new Color(color.r, color.g, color.b, optionsWindow._background / 100.0f);
                position = GUILayout.Window(id, position, MainWindow, title, style);
                GUI.color = color;
            }

            RenderControls();
        }

        private void RenderControls()
        {
            if (show && controls)
            {
                Color color = GUI.color;
                GUI.color = new Color(color.r, color.g, color.b, optionsWindow._control / 100.0f);
                GUI.depth--;
                _close = GUI.Button(new Rect(position.xMax - 20, position.yMin + 8, 12, 12), "", _closeStyle);
                _minimize = GUI.Button(new Rect(position.xMax - 40, position.yMin + 8, 12, 12), "", _minimizeStyle);
                _options = GUI.Button(new Rect(position.xMax - 60, position.yMin + 8, 12, 12), "", _optionsStyle);
                GUI.color = color;

                if (_options)
                {
                    optionsWindow.Toggle();
                }

                if (_minimize)
                {
                    if (!_minimizeWindow)
                        SetSize(width, _minimizeHeight, false);
                    else
                        SetSize(width, height, false);

                    _minimizeWindow = !_minimizeWindow;
                }

                if (_close)
                {
                    Hide();
                }
            }
        }

        private void MainWindow(int id)
        {
            Color color = GUI.color;
            if (controls)
                GUI.color = new Color(color.r, color.g, color.b, optionsWindow._content / 100.0f);
            GUILayout.BeginHorizontal();
            if (!_minimizeWindow)
                WindowElements();
            GUILayout.EndHorizontal();
            GUI.color = color;

            GUI.DragWindow();
        }

        public virtual void WindowElements()
        {
            throw new NotImplementedException();
        }

        public void OnDestroy()
        {
            if (controls)
                Destroy(optionsWindow);
        }
    }
}
