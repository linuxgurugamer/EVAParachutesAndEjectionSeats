﻿using System;
using System.Diagnostics;
using UnityEngine;

namespace VanguardTechnologies
{

    public static class Log
    {
        public enum LEVEL
        {
            OFF = 0,
            ERROR = 1,
            WARNING = 2,
            INFO = 3,
            DETAIL = 4,
            TRACE = 5
        };
        private static string TITLE = "VanguardTechnologies";
        public static LEVEL level = LEVEL.INFO;

        private static readonly String PREFIX = TITLE + ": ";

        public static void SetTitle(string str)
        {
            TITLE = str;
        }

        public static LEVEL GetLevel()
        {
            return level;
        }

        [ConditionalAttribute("DEBUG")]
        public static void SetLevel(LEVEL level)
        {
            UnityEngine.Debug.Log("log level " + level);
            Log.level = level;
        }


        public static LEVEL GetLogLevel()
        {
            return level;
        }


        private static bool IsLevel(LEVEL level)
        {
            return level == Log.level;
        }


        public static bool IsLogable(LEVEL level)
        {
            return level <= Log.level;
        }

        [ConditionalAttribute("DEBUG")]
        public static void Trace(String msg)
        {
            if (IsLogable(LEVEL.TRACE))
            {
                UnityEngine.Debug.Log(PREFIX + msg);
                //			MonoBehaviour.print ("[" + name + ":" + level + ":" + id + "]  " + message);

            }
        }

        [ConditionalAttribute("DEBUG")]
        public static void Detail(String msg)
        {
            if (IsLogable(LEVEL.DETAIL))
            {
                UnityEngine.Debug.Log(PREFIX + msg);
            }
        }

        [ConditionalAttribute("DEBUG")]
        public static void Info(String msg)
        {
            if (IsLogable(LEVEL.INFO))
            {
                UnityEngine.Debug.Log(PREFIX + msg);
            }
        }

        [ConditionalAttribute("DEBUG")]
        public static void Test(String msg)
        {
            //if (IsLogable(LEVEL.INFO))
            {
                UnityEngine.Debug.LogWarning(PREFIX + "TEST:" + msg);
            }
        }

        [ConditionalAttribute("DEBUG")]
        public static void Debug(String msg)
        {
            //if (IsLogable(LEVEL.INFO))
            {
                UnityEngine.Debug.LogWarning(PREFIX + "DEBUG:" + msg);
            }
        }


        public static void Warning(String msg)
        {
            if (IsLogable(LEVEL.WARNING))
            {
                UnityEngine.Debug.LogWarning(PREFIX + msg);
            }
        }

        public static void Error(String msg)
        {
            if (IsLogable(LEVEL.ERROR))
            {
                UnityEngine.Debug.LogError(PREFIX + msg);
            }
        }

        public static void Exception(Exception e)
        {
            Log.Error("exception caught: " + e.GetType() + ": " + e.Message);
        }

    }

}