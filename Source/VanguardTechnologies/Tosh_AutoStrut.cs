/* Name: Tosh_AutoStrut Plugin for KSP 0.16
   Version: 1.0.
   Author: Tony Chernigvsky, SPb, Russia. 
     mailto: tosh@bk.ru?subject=Tosh_AutoStrut
   License:
     BY, Attribution Creative Common Licence. Free for ANY use as 
     long as an original author is explicitly mentioned.
     AS IS: use at your risk, no guarantee is provided. And do not 
     cry if something breaks ;).
   See README.TXT for project details and building instructions. */

using UnityEngine;
using System;
using System.Collections.Generic;

#if false

namespace VanguardTechnologies
{
    public class Tosh_AutoStrut : PartModule
    {
        [KSPField(isPersistant = false)]
        public bool rotateAnchor = true, tileStrutTexture = true, detectManInTheMiddle = true, connectToParent = false, connectToCounterparts = true;

        [KSPField(isPersistant = false)]
        public string replacementStrutShader = "";

        // A single strut, along with anchors, transforms, scales and 
        // physical joints.
        private class Strut
        {
            private Tosh_AutoStrut mOwner = null;
            private Transform mTransform = null;

            private Transform mAnchor = null;
            private Transform mStrut = null;

            private Strut mTarget = null;
            private bool mReceiving = false;
            private FixedJoint mJoint = null;

            private Mesh mStrutMesh = null;
            private Material mStrutMaterial = null;
            private Vector3 mUp = Vector3.zero;
            private float mOriginalScale = 0;
            private Vector2 mMainTexScale, mBumpTexScale;

            Renderer anchorRenderer = null;
            Renderer strutRenderer = null;
            Rigidbody ownerRigidbody = null;

            public Strut(Tosh_AutoStrut AOwner, Transform strut, Transform anchor)
            {
                mOwner = AOwner;
                mTransform = owner.part.transform;
                mAnchor = anchor;
                mStrut = strut;

                if (AOwner != null)
                    ownerRigidbody = AOwner.GetComponent<Rigidbody>();

                //            if (mAnchor && anchorRenderer)
                //                anchorRenderer.enabled = false;
                if (mAnchor)
                {
                    anchorRenderer = mAnchor.GetComponent<Renderer>();
                    if (anchorRenderer)
                        anchorRenderer.enabled = false;
                }

                if (mStrut)
                {
                    strutRenderer = mStrut.GetComponent<Renderer>();
                    if (strutRenderer)
                    {
                        strutRenderer.enabled = false;

                        if (owner.replacementStrutShader != "")
                        {
                            Shader s = Shader.Find(owner.replacementStrutShader);
                            if (s)
                                strutRenderer.material.shader = s;
                        }

                        mStrutMaterial = strutRenderer.material;
                        mMainTexScale = mStrutMaterial.GetTextureScale("_MainTex");
                        mBumpTexScale = mStrutMaterial.GetTextureScale("_BumpMap");
                    }

                    MeshFilter f = mStrut.GetComponent<MeshFilter>();
                    if (f)
                        mStrutMesh = f.mesh;

                    // FIXME. "mUp" may point just anywhere While assembling a craft 
                    // in the VAB (though everything's generally fine during the 
                    // flight). I need some other way to determine "initial up" 
                    // direction :(
                    mUp = mTransform.InverseTransformDirection(mStrut.up);
                    mOriginalScale = mStrut.localScale.z;
                }

                Debug.Log("Tosh_AutoStrut found " + this);
            }

            public Tosh_AutoStrut owner
            {
                get { return mOwner; }
            }

            public Strut target
            {
                get { return mTarget; }
            }

            private Rigidbody rigidbody
            {
                get { return mOwner ? ownerRigidbody : null; }
            }

            public Vector3 position
            {
                get { return mStrut ? mStrut.position : Vector3.zero; }
            }

            public void ConnectTo(Strut strut)
            {
                if (!strut)
                    return;

                mTarget = strut;
                mTarget.mTarget = this;
                mReceiving = false;
                mTarget.mReceiving = true;

                if (mAnchor && anchorRenderer)
                    anchorRenderer.enabled = true;
                if (target.mAnchor && target.anchorRenderer)
                    target.anchorRenderer.enabled = true;

                if (mStrut && strutRenderer)
                    strutRenderer.enabled = true;

                Debug.Log("Tosh_AutoStrut connecting " + this + " to " + target);
            }

            public void BreakOff()
            {
                DestroyJoint();

                if (!target)
                    return;

                if (mReceiving)
                {
                    target.BreakOff();
                }
                else
                {
                    Debug.Log("Tosh_AutoStrut breaking " + this + " from " + target);
                    mTarget.mTarget = null;
                }

                if (mStrut && strutRenderer)
                    strutRenderer.enabled = false;
                if (mAnchor && anchorRenderer)
                    anchorRenderer.enabled = false;

                mTarget = null;
            }

            public void UpdateVisual()
            {
                if ((!target) || (!owner))
                    return;

                CreateJoint(); // ...if not already created.

                Vector3 targetPos = target.position - position;
                Quaternion rot = Quaternion.LookRotation(
                    targetPos.normalized, mTransform.TransformDirection(mUp));

                if (mStrut)
                {
                    mStrut.rotation = rot;

                    if (mStrutMesh)
                    {
                        Vector3 s = mStrut.localScale;
                        float z = mStrutMesh.bounds.size.z;
                        if (z > 0)
                        {
                            s.z = targetPos.magnitude / z / mOwner.part.rescaleFactor;
                            mStrut.localScale = s;

                            if (owner.tileStrutTexture)
                            {
                                Vector2 v = new Vector2(s.z / mOriginalScale, 1);
                                mStrutMaterial.SetTextureScale("_MainTex",
                                    Vector2.Scale(mMainTexScale, v));
                                mStrutMaterial.SetTextureScale("_BumpMap",
                                    Vector2.Scale(mBumpTexScale, v));
                            }
                        }
                    }
                }

                if (mAnchor && owner.rotateAnchor)
                    mAnchor.rotation = rot;
            }

            public void CreateJoint()
            {
                if ((!mStrut) || (!target) || (!target.mStrut) || mReceiving)
                    return;
                if ((!rigidbody) || rigidbody.isKinematic ||
                    (!target.rigidbody) || target.rigidbody.isKinematic)
                    return;
                if (mJoint)
                    return;

                Debug.Log("Tosh_AutoStrut creating joint from " + this + " to " + target);

                mJoint = owner.gameObject.AddComponent<FixedJoint>();
                mJoint.connectedBody = target.rigidbody;
                mJoint.breakForce = Mathf.Infinity;
                mJoint.breakTorque = Mathf.Infinity;
            }

            public void DestroyJoint()
            {
                if (mJoint)
                    DestroyObject(mJoint);
                mJoint = null;
            }

            public static implicit operator bool(Strut v)
            {
                return v != null;
            }

            public static implicit operator string(Strut v)
            {
                string s = "[";

                if (v)
                {
                    if (v.owner)
                        s += v.owner.id;
                    else
                        s += "-";
                    s += ":";
                    if (v.mStrut)
                        s += v.mStrut.name;
                    else
                        s += "-";
                }
                else
                {
                    s += "-:-";
                }

                return s + "]";
            }
        }

        // List of all the struts belonging to the part.
        private class Struts : List<Strut>
        {

            public void BreakOff()
            {
                foreach (Strut s in this)
                    s.BreakOff();
            }

            private StrutConfig GetConfig()
            {
                StrutConfig cfg = new StrutConfig();
                foreach (Strut s in this)
                    if (s.target)
                        cfg.Add(s, s.target, 0);
                return cfg;
            }

            private void SetConfig(StrutConfig cfg)
            {
                foreach (StrutConnection s in cfg)
                    s.from.ConnectTo(s.to);
            }

            public StrutConfig config
            {
                get { return GetConfig(); }
                set
                {
                    StrutConfig cfg = GetConfig();
                    if (value && !value.Equals(cfg))
                    {
                        Debug.Log("Tosh_AutoStrut changed: was" + cfg + "now" + value);
                        BreakOff();
                        SetConfig(value);
                    }
                }
            }

            public void UpdateVisual()
            {
                foreach (Strut s in this)
                    s.UpdateVisual();
            }

            public void DestroyJoints()
            {
                foreach (Strut s in this)
                    s.DestroyJoint();
            }

        }

        // Description of a single strut link. Used when determining desired
        // strut configuration and comparing it with current one.
        private class StrutConnection
        {
            public Strut from, to;
            public float length;

            public StrutConnection(Strut AFrom, Strut ATo, float ALength)
            {
                from = AFrom;
                to = ATo;
                length = ALength;
            }

            public bool Equals(StrutConnection v)
            {
                return v &&
                       (((v.from == from) && (v.to == to)) ||
                         ((v.from == to) && (v.to == from)));
            }

            public static implicit operator bool(StrutConnection v)
            {
                return v != null;
            }

            public static implicit operator string(StrutConnection v)
            {
                if (v)
                    return "(" + v.from + "->" + v.to + ")";
                else
                    return "(-)";
            }
        }

        // List of all the strut links. Allows to easily add a new record 
        // (ensuring that it's _really_ "new"), find a link between two 
        // given parts, and compare the list with another one.
        private class StrutConfig : List<StrutConnection>
        {

            public void Add(Strut from, Strut to, float length)
            {
                StrutConnection c = new StrutConnection(from, to, length);
                if (!Find(c))
                {
                    // no duplicate link already listed
                    int i = IndexOf(from.owner, to.owner);
                    if (i < 0)
                        // no linking same part twice
                        base.Add(c);
                    else if (c.length < this[i].length)
                        // new link is shorter. Replace.
                        this[i] = c;
                }
            }

            public bool Equals(StrutConfig v)
            {
                if ((!v) || (v.Count != Count))
                    return false;

                foreach (StrutConnection c in this)
                    if (!v.Find(c))
                        return false;

                return true;
            }

            public new int IndexOf(StrutConnection what)
            {
                for (int i = 0; i < Count; i++)
                    if (this[i] && this[i].Equals(what))
                        return i;
                return -1;
            }

            public int IndexOf(Tosh_AutoStrut us, Tosh_AutoStrut them)
            {
                for (int i = 0; i < Count; i++)
                {
                    StrutConnection c = this[i];
                    if (c && c.from && c.to &&
                        (((c.from.owner == us) && (c.to.owner == them)) ||
                          ((c.from.owner == them) && (c.to.owner == us))))
                        return i;
                }
                return -1;
            }

            public StrutConnection Find(StrutConnection what)
            {
                int i = IndexOf(what);
                if (i < 0)
                    return null;
                else
                    return this[i];
            }

            public StrutConnection Find(Tosh_AutoStrut us, Tosh_AutoStrut them)
            {
                int i = IndexOf(us, them);
                if (i < 0)
                    return null;
                else
                    return this[i];
            }

            public static implicit operator bool(StrutConfig v)
            {
                return v != null;
            }

            public static implicit operator string(StrutConfig v)
            {
                string s = " ";
                if (v && (v.Count > 0))
                    foreach (StrutConnection c in v)
                        s += c + "; ";
                else
                    s += "blank; ";
                return s;
            }
        }

        private class Counterparts : List<Tosh_AutoStrut> { }

        private Struts mStruts = new Struts();

        static private int cnt = 0;
        private string id = "";

        // Detect all the strut objects present in the owning part.
        public override void OnStart(StartState s)
        {
            base.OnStart(s);

            cnt++;
            id = part.name + "#" + cnt;

            foreach (Transform t in part.transform.Find("model"))
                if (t.name.StartsWith("strut"))
                {
                    string name = t.name.Substring(5);
                    Transform anchor = t.parent.Find("anchor" + name);
                    mStruts.Add(new Strut(this, t, anchor));
                }
        }

        private Part FromCollider(Collider c)
        {
            GameObject o = c.gameObject;
            Part p = null;
            while (o)
            {
                if ((p = Part.FromGO(o)) != null)
                    break;

                if (o.transform.parent)
                    o = o.transform.parent.gameObject;
                else
                    break;
            }
            return p;
        }

        private Tosh_AutoStrut GetMe(Part p)
        {
            if (p && p.transform.Find("model"))
                // if the part is at least initialized...
                foreach (PartModule m in p.Modules)
                    if (m is Tosh_AutoStrut)
                        return m as Tosh_AutoStrut;
            return null;
        }

        // "Men-in-the-middle" support. WARNING! EXPERIMENTAL!
        // Each frame an Tosh_AutoStrut:
        // * locates all its symmetry counterparts,
        // * raycasts to them and detects auto-strutted parts in between,
        // * each new part is added as an Tosh_AutoStrut to which we may connect 
        //   to,
        // * moreover, Tosh_AutoStrut registers itself in detected part's 
        //   "MenInMiddle" list, so that detected one knows who wants to 
        //   connect,
        // * that list lives for one frame only ("mMenAlive" list). If some
        //   part brakes off, on next frame it won't register in the list 
        //   again, and will no longer be connected to.

        private Counterparts mMenInMiddle = new Counterparts();
        private List<bool> mMenAlive = new List<bool>();

        private Counterparts GetMenInTheMiddle(Counterparts list)
        {
            Counterparts c = new Counterparts();

            if (!part)
                return c;

            foreach (Tosh_AutoStrut m in list)
                foreach (Strut s2 in m.mStruts)
                    foreach (Strut s1 in mStruts)
                    {
                        Vector3 v = s2.position - s1.position;
                        foreach (RaycastHit r in Physics.RaycastAll(s1.position, v.normalized, v.magnitude))
                        {
                            Part p = FromCollider(r.collider);
                            if (p && p != part && p.vessel == part.vessel)
                            {
                                Tosh_AutoStrut a = GetMe(p);
                                if (a && !list.Contains(a) && !c.Contains(a))
                                    c.Add(a);
                            }
                        }
                    }

            return c;
        }

        // "Revive" the men which are still between the symmetry counterparts 
        // and thus should live for one more frame.
        private void ReviveMen(Counterparts list)
        {
            foreach (Tosh_AutoStrut m in list)
                if (!m.mMenInMiddle.Contains(this))
                {
                    m.mMenInMiddle.Add(this);
                    m.mMenAlive.Add(true);
                }
                else
                {
                    m.mMenAlive[m.mMenInMiddle.IndexOf(this)] = true;
                }
        }

        // Kill the men not detected on this frame (and thus no more available 
        // for connection).
        private void RemoveDeadMen()
        {
            int i = 0;
            while (i < mMenInMiddle.Count)
                if ((!mMenInMiddle[i]) ||
                    (!mMenInMiddle[i].part) ||
                    (mMenInMiddle[i].part.vessel != part.vessel) ||
                    !mMenAlive[i])
                {
                    mMenInMiddle.RemoveAt(i);
                    mMenAlive.RemoveAt(i);
                }
                else
                {
                    mMenAlive[i] = false;
                    i++;
                }
        }

        // And here's where we get ALL the parts we can connect to.
        private Counterparts GetCounterparts()
        {
            Counterparts s = new Counterparts();
            if (!part)
                return s;
            if (connectToCounterparts)
                foreach (Part p in part.symmetryCounterparts)
                {
                    Tosh_AutoStrut m = GetMe(p);
                    if (m && !s.Contains(m))
                        s.Add(m);
                }

            if (connectToParent && GetMe(part.parent))
                s.Add(GetMe(part.parent));

            Tosh_AutoStrut a;
            foreach (Part p in part.children)
            {
                a = GetMe(p);
                if (a && a.connectToParent)
                    s.Add(a);
            }

            if (detectManInTheMiddle)
            {
                Counterparts middle = GetMenInTheMiddle(s);
                ReviveMen(middle);
                s.AddRange(middle);

                RemoveDeadMen();
                s.AddRange(mMenInMiddle);
            }


            return s;
        }

        // Determine a "desired" strut configuration. If it differs from 
        // current one, then the struts need to be rebuilt.
        private StrutConfig DesiredConfig()
        {
            StrutConfig cfg = new StrutConfig();
            Counterparts counterparts = GetCounterparts();

            // FIXME: 'naive' algorythm glitches a lot.
            // A better solution would be to minimize a sum of connection lengths 
            // amongst all the configurations possible. 

            foreach (Strut s in mStruts)
            {

                float min = Mathf.Infinity;
                Strut cMin = null;

                foreach (Tosh_AutoStrut c in counterparts)
                    foreach (Strut cs in c.mStruts)
                    {
                        float v = (s.position - cs.position).sqrMagnitude;
                        if (v < min)
                        {
                            min = v;
                            cMin = cs;
                        }
                    }

                if (cMin)
                    cfg.Add(s, cMin, min); // 'add' method eliminates double links 
                                           // and replaces longer ones automatically.
            }

            return cfg;
        }

        // Anticipate a strut config change in each frame. We may have 
        // symmetry mode changed in editor, or some of the counterparts
        // dropped off in flight.
        // Assigning this config to mStruts will do nothing, if that 
        // config is effectively the same as the current one.
        public void Update()
        {
            mStruts.config = DesiredConfig();
            mStruts.UpdateVisual();
        }

        public void OnDestroy()
        {
            mStruts.BreakOff();
        }
    }
}
#endif