using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This doesn't work now
#if false
namespace VanguardTechnologies
{
    [KSPAddon(KSPAddon.Startup.Settings, false)]
    class AssetDatabasePrinter : MonoBehaviour
    {
        public void OnGUI()
        {
            if (GUILayout.Button("Vanguard Technologies Asset Database Printer - save asset list to kspdir/assetlist.log"))
            {
                ConfigNode topNode = new ConfigNode("ASSETS");
                AssetBase assetBase = (AssetBase)UnityEngine.Object.FindObjectOfType(typeof(AssetBase));

                ConfigNode guiSkinNode = new ConfigNode("GUISKINS");
                foreach (GUISkin s in assetBase.guiSkins)
                    guiSkinNode.AddValue("objectName", s.name);

                ConfigNode prefabNode = new ConfigNode("PREFABS");
                if (assetBase != null)
                    foreach (GameObject o in assetBase.prefabs)
                     prefabNode.AddValue("objectName", o.name);

                ConfigNode textureNode = new ConfigNode("TEXTURES");
                if (assetBase != null)
                    foreach (Texture2D t in assetBase.textures)
                        textureNode.AddValue("objectName", t.name);

                ConfigNode unityResource = new ConfigNode("UNITYRESOURCES");
                int nameless = 0, unass = 0, newGameObject = 0;
                foreach (UnityEngine.Object o in UnityEngine.Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)))
                    switch (o.name)
                    {
                        case "": nameless++; break;
                        case "Unass": unass++; break;
                        case "New Game Object": newGameObject++; break;
                        default:
                            unityResource.AddValue("objectName", o.name);
                            break;
                    }
                unityResource.AddValue("nameless", nameless);
                unityResource.AddValue("Unass", unass);
                unityResource.AddValue("NewGameObject", newGameObject);

                topNode.AddNode(guiSkinNode);
                topNode.AddNode(prefabNode);
                topNode.AddNode(textureNode);
                topNode.AddNode(unityResource);
                topNode.Save(KSPUtil.ApplicationRootPath + "/assetlist.log");
            }
        }
    }
}
#endif