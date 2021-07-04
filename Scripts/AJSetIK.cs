using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AJSetIK : MonoBehaviour
{
    public Animator animator;
    public Transform point;
    [Range(0, 1f)]
    public float pointweight;
    [Range(0, 1f)]
    public float point1weight;
    public Transform point1;
    [Range(0,1f)]
    public float GroundLine = 1f;
    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    //private void OnAnimatorIK(int layerIndex)
    //{
    //    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
    //    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
    //    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
    //    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
    //    var lfp = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
    //    var rfp = animator.GetIKPosition(AvatarIKGoal.RightFoot);
    //    var newlfp = CheckGround(lfp);
    //    var newrfp = CheckGround(rfp);
    //    if(newlfp!=null)
    //    {
    //        animator.SetIKPosition(AvatarIKGoal.LeftFoot, newlfp[0]);
    //        animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, newlfp[1]));
    //    }
    //    if (newrfp != null)

    //    {
    //        animator.SetIKPosition(AvatarIKGoal.RightFoot, newrfp[0]);
    //        animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, newrfp[1]));
    //    }
    //}

    //Vector3[] CheckGround(Vector3 point)
    //{
    //    RaycastHit hit;
    //    Ray ray = new Ray(point + Vector3.up, Vector3.down);
    //    Debug.DrawRay(point, transform.forward,Color.green);
    //    if(Physics.Raycast(ray,out hit,GroundLine*100,1<<LayerMask.NameToLayer("Block")))
    //    {
    //        Vector3 p = hit.point;
    //        p.y += GroundLine;
    //        return new Vector3[] { p, hit.normal };
    //    }
    //    return null;
    //}

    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, point1weight);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, pointweight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, point.position);
        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, point1.position);
    }
}
