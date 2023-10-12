using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ConnectTransform : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            if(!target) return;

            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}