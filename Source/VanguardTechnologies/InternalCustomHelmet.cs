using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if false
namespace VanguardTechnologies
{
    class InternalKrCustomHelmet : InternalModule
    {
        [KSPField]
        public string modelPath = "Squad/Parts/Electrical/RTG/model" /*haha*/, modelName = "customHelmet", transformPath = "kbIVA@idle/globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01";
        [KSPField]
        public UnityEngine.Vector3 localPosition = UnityEngine.Vector3.zero, scale = UnityEngine.Vector3.one, localRotation = UnityEngine.Vector3.zero;
        [KSPField]
        public bool debugTransformPath = false;


        public override void OnUpdate()
        {
            foreach (Kerbal k in internalModel.transform.GetComponentsInChildren<Kerbal>())
                if (!k.transform.Find(transformPath + "/" + modelName))
                {
                    GameObject helmet = GameDatabase.Instance.GetModel(modelPath);
                    helmet.transform.name = modelName;
                   // UIPartActionController.SetLayerRecursive(helmet.transform, 16);
                    helmet.SetActive(true);
                    helmet.transform.parent = k.transform.Find(transformPath);
                    if (debugTransformPath)
                        CheckTheF___ingTransforms(k.transform, transformPath);
                    Log.Info(helmet.transform.parent.ToString());
                    helmet.transform.localPosition = localPosition;
                    helmet.transform.localScale = scale;
                    helmet.transform.localRotation = Quaternion.Euler(localRotation);
                }
        }

        void CheckTheF___ingTransforms(Transform t, string path)
        {
            foreach (Transform c in t)
            {
                Log.Info(c.name);
                if (path.StartsWith(c.name))
                {
                    CheckTheF___ingTransforms(c, path.Remove(0, path.IndexOf('/') + 1));
                    return;
                }
            }
            Log.Info("---END OF TRANSFORM CHECK---");
        }
    }
}
#endif