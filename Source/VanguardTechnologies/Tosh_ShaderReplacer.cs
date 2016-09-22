/* Name: shaderReplacer Plugin for KSP 0.16
   Version: 1.0.
   Author: Tony Chernigvsky, SPb, Russia. 
     mailto: tosh@bk.ru?subject=shaderReplacer
   License:
     BY, Attribution Creative Common Licence. Free for ANY use as 
     long as an original author is explicitly mentioned.
     AS IS: use at your risk, no guarantee is provided. And do not 
     cry if something breaks ;).
   Usage:
     Add the following lines to PART.CFG file.

       MODULE
       {
          name = shaderReplacer
          obj = <fully-qualified object name (or an empty string for root object)>
          shader = <fully-qualified shader name (see Unity documentation)>
       }   

     For example,

       MODULE
       {
          name = shaderReplacer
          obj = Body/Arm/Finger
          shader = Transparent/Cutout/Bumped Specular
       }   
*/

using UnityEngine;
using System;
using System.Collections.Generic;

#if false
namespace VanguardTechnologies
{
    public class Tosh_ShaderReplacer : PartModule
    {
        [KSPField]
        public string obj;

        [KSPField]
        public string shader;

        public override void OnStart(StartState s)
        {
            base.OnStart(s);

            Transform t;
            if (part && ((t = part.transform.Find("model")) != null))
            {
                if (obj == "")
                    obj = "root";
                else
                    t = t.Find(obj);

                if (!t)
                    Debug.Log("shaderReplacer @" + part.name + ": cannot find object " + obj);
                //else if (!t.renderer)
                //Debug.Log("shaderReplacer @" + part.name + ": object " + obj + " has no renderer");
                else
                {
                    Shader h = Shader.Find(shader);
                    if (!h)
                        Debug.Log("shaderReplacer @" + part.name + ": cannot find shader " + shader);
                    //else
                    //{
                    //    Debug.Log("shaderReplacer @" + part.name + ": setting " + shader + " shader for " + obj);
                    //    t.renderer.material.shader = h;
                    //}
                }
            }
        }
    }
}
#endif