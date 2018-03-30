using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Shows when which override of PartModule is called
/// </summary>
namespace VanguardTechnologies
{
    class ModuleKrTestSpam : PartModule
    {
        public override void OnActive()
        {
            Log.Info("OnActive");
        }
        public override void OnAwake()
        {
            Log.Info("OnAwake");
        }
        public override void OnInactive()
        {
            Log.Info("OnInactive");
        }
        public override void OnLoad(ConfigNode node)
        {
            Log.Info("OnLoad");
        }
        public override void OnFixedUpdate()
        {
            Log.Info("OnFixedUpdate");
        }
        public override void OnUpdate()
        {
            Log.Info("OnUpdate");
        }
        public override void OnSave(ConfigNode node)
        {
            Log.Info("OnSave");
        }
        public override void OnStart(StartState state)
        {
            Log.Info("OnStart:" + state);
        }
    }
}