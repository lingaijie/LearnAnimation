using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCDIK : MonoBehaviour
{
    public List<Transform> bones = new List<Transform>();
    public Transform lastPoint;
    public Transform targetPoint;
    private float maxLength = 0f;
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
    }

    private void FixedUpdate()
    {
        DoCCDIK();
    }

    void DoCCDIK()
    {
        for(int i=bones.Count-2;i>=0;i--)
        {
            var c = lastPoint.position - bones[i].position;
            var t = targetPoint.position - bones[i].position;
            var r = Quaternion.FromToRotation(c, t);
            bones[i].rotation = r * bones[i].rotation; 
        }
    }
}
