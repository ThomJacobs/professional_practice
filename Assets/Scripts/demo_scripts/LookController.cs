using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Jacobs.Core
{
    [System.Serializable]
    public struct Range
    {
        [Range(0.0f, 360.0f)] public float m_minimum;
        [Range(0.0f, 360.0f)] public float m_maximum;
    }

    [RequireComponent(typeof(Camera))]
    public sealed class LookController : NetworkBehaviour
    {
        //Attributes:
        [Header("Main Settings")]
        [SerializeField] private float m_verticalSensitivity = 100.0f;
        [SerializeField] private bool m_isInverted = false;
        public float YRotation = 0.0f;
        private const float EULER_MAXIMUM = 85.0f;

        //Properties:
        private float MouseX => Input.GetAxis("Mouse Y");
        private float MouseY => m_isInverted ? Input.GetAxis("Mouse Y") : -Input.GetAxis("Mouse Y");

        //Methods:
        private void Update()
        {
            if (!IsLocalPlayer) return;

            //Limit the rotation.
            YRotation = Mathf.Clamp(YRotation + MouseY * m_verticalSensitivity * Time.deltaTime, -EULER_MAXIMUM, EULER_MAXIMUM);
            
            //Apply the rotation.
            transform.localRotation = Quaternion.Euler(YRotation, 0.0f, 0.0f);
        }

        #region FOR_DEVELOPMENT_ONLY
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            const float m_length = 2.0f;
        }
#endif
        #endregion
    }
}