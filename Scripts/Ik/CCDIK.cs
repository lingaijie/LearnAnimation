using System.Collections.Generic;
using Ik;
using UnityEngine;

public class CCDIK : MonoBehaviour
{
    public List<Transform> bones = new List<Transform>();
    public Transform lastPoint;
    public Transform targetPoint;
    private float maxLength = 0f;
    private List<Bone> Bones = new List<Bone>();
    private Vector3 MinEularAngels = Vector3.zero;
    private Vector3 MaxEularAngels=new  Vector3(30,0,0);
    void Start()
    {
        if(bones.Count>1)
        {
            lastPoint = bones[bones.Count - 1];
        }
        for(int i = 0; i < bones.Count - 1; i++)
        {
            maxLength += Vector3.Distance(bones[i].position, bones[i+1].position);
        }
        Debug.Log(maxLength);
        InitBones();
    }

    void InitBones()
    {
        for (int i = 0; i < bones.Count; i++)
        {
            Bone bone=new Bone();
            bone.Init(bones[i].transform, new Vector3(0, 0, 0), new Vector3(30, 0, 0));
            Bones.Add(bone);
        }
    }

    private void LateUpdate()
    {
        DoCCDIK();
        //DOCCDIK();
    }

    void DoCCDIK()
    {
        for(int i=bones.Count-2;i>=0;i--)
        {
            var c = lastPoint.position - bones[i].position;
            var t = targetPoint.position - bones[i].position;
            var r = Quaternion.FromToRotation(c, t);
            if (i == 0)
            {
                Vector3 axis = GetAxis(bones[i].rotation);
                var crp = bones[i].rotation * axis;
                var prp = bones[i].parent.rotation * axis;
                var c2pp = Quaternion.FromToRotation(crp, prp);
                bones[i].rotation =
                    Quaternion.FromToRotation(bones[i].rotation * axis, bones[i].parent.rotation * axis) *
                    bones[i].rotation;
            }
            r.eulerAngles = new Vector3(r.eulerAngles.x,0f,0f);
            bones[i].rotation = r * bones[i].rotation;
        }
    }

    void DOCCDIK()
    {
        for(int i=Bones.Count-2;i>=0;i--)
        {
            var c = lastPoint.position - Bones[i].transform.position;
            var t = targetPoint.position - Bones[i].transform.position;
            var r = Quaternion.FromToRotation(c, t);
            //r.eulerAngles = GetVector3(r.eulerAngles, Bones[i].mineularAngels, Bones[i].maxeularAngels);
            Bones[i].transform.rotation = r * bones[i].rotation;
        }
    }

    Vector3 GetAxis(Quaternion q)
    {
        Vector3 axis = Vector3.zero;
        float sint = Mathf.Sqrt(1 - Mathf.Pow(q.w, 2));
        if (sint == 0f)
        {
            return axis;
        }
        axis.x = q.x / sint;
        axis.y = q.y / sint;
        axis.z = q.z / sint;
        return axis;
    }

    Vector3 GetVector3(Vector3 v1, Vector3 minv,Vector3 maxv)
    {
        Vector3 v = Vector3.zero;
        v.x = Mathf.Max(Mathf.Min(v1.x, maxv.x),minv.x);
        v.y = Mathf.Max(Mathf.Min(v1.y, maxv.y),minv.y);
        v.z = Mathf.Max(Mathf.Min(v1.z, maxv.z),minv.z);
        return v;
    }
}
