using UnityEngine;

namespace Ik
{
    public class Bone
    {
        public bool limit;
        public bool activity;
        public Vector3 localPos;
        public Vector3 globalPos;
        public Vector3 mineularAngels;
        public Vector3 maxeularAngels;
        public Transform transform;

        public void Init(Transform transform,Vector3 mineularAngels,Vector3 maxeularAngeles)
        {
            this.transform = transform;
            this.mineularAngels = mineularAngels;
            this.maxeularAngels = maxeularAngeles;
        }
    }
}