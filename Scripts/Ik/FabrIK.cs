using System.Collections.Generic;
using UnityEngine;

public class FabrIK : MonoBehaviour
{
    public List<Transform> bones = new List<Transform>();
    public Transform targetPoint;
    private float dis;
    private int count;
    private Vector3 root;
    bool state = true;
    Vector3 lastPoint = Vector3.zero;
    void Start()
    {
        dis = Vector3.Distance(bones[0].position, bones[1].position);
        root = bones[0].position;
        count = bones.Count - 1;
    }
    private Vector3 drawPoint;
    void DoFabrIK()
    {
        if (state)
        {
            var tpoint = targetPoint.position;
            if (Vector3.Distance(root, targetPoint.position) > dis * count)
            {
                tpoint = (targetPoint.position - root).normalized * count * dis + root;
            }
            drawPoint = tpoint;
            if (Vector3.Distance(tpoint, bones[0].position) < 1f) return;
            for (int i = bones.Count - 1; i > 0; i--)
            {
                bones[i].position = tpoint;
                tpoint = (bones[i - 1].position - bones[i].position).normalized * dis + bones[i].position;
            }
            bones[0].position = tpoint;
            state = false;
        }
        else
        {
            var tpoint = root;
            for (int i = 0; i < bones.Count - 1; i++)
            {
                bones[i].position = tpoint;
                tpoint = (bones[i + 1].position - bones[i].position).normalized * dis + bones[i].position;
            }
            bones[bones.Count - 1].position = tpoint;
            state = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(drawPoint, 0.5f);
    }

    private void LateUpdate()
    {
        DoFabrIK();
    }
}
