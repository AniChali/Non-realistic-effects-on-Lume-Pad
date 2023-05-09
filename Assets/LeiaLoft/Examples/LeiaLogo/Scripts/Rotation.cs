using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft.Examples
{
    public class Rotation : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation = Vector3.zero;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}
