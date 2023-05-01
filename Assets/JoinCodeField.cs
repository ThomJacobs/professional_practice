using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class JoinCodeField : NetworkBehaviour
{
    //Attribute:
    [SerializeField] private TMPro.TMP_InputField m_inputField = null;
    [SerializeField] private Button m_button = null;

    //Methods:
    private void Awake()
    {
        if(m_inputField == null || m_button == null)
        {
            Debug.LogError("[JoinCodeField] Component fields have not been initialised/setup correctly.");
            Destroy(this);
            return;
        }

        //Setup components.
        m_button.onClick.AddListener(delegate { StartSession(m_inputField.text); });
    }

    public void StartSession(string p_joinCode)
    {
        try
        {
            Jacobs.Core.RelayManager.Singleton.JoinServer(p_joinCode);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}