using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Underground.Core
{
    public class PlayerState : NetworkBehaviour
    {
        //Attributes:
        [HideInInspector] private bool m_IsAlive = false;
        public event System.Action<ulong, bool> OnValueChange;

        //Properties:

        /**
         * Get or set the value of 'm_IsAlive, any methods/functions subscribed to 'OnValueChanged' will be invoked when
         * the setter is invoked.
         */
        public bool IsAlive
        {
            get => m_IsAlive;
            set
            {
                m_IsAlive = value;
                OnValueChange?.Invoke(OwnerClientId, m_IsAlive);
            }
        }

        //Methods:
        private void Awake() => m_IsAlive = true;

        public override void OnDestroy()
        {
            //base.OnDestroy();
            m_IsAlive = false;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PlayerState))] public class PlayerStateEditor : UnityEditor.Editor
    {
        //Attributes:
        private PlayerState m_Self = null;

        //Methods:
        private void Awake()
        {
            m_Self = (PlayerState)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UnityEditor.EditorGUILayout.LabelField("Developer Settings", UnityEditor.EditorStyles.boldLabel);
            if(UnityEngine.GUILayout.Button("Toggle State"))
            {
                m_Self.IsAlive = !m_Self.IsAlive;
            }
        }
    }
#endif
}