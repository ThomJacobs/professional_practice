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
    [SerializeField] private TMPro.TMP_InputField m_nameInputField = null;
    [SerializeField] private Button m_button = null;

    //Methods:
    private void Awake()
    {
        if(m_inputField == null || m_button == null || m_nameInputField == null)
        {
            Debug.LogError("[JoinCodeField] Component fields have not been initialised/setup correctly.");
            Destroy(this);
            return;
        }

        //Setup components.
        m_button.onClick.AddListener(delegate { StartSession(m_inputField.text, m_nameInputField.text); });
    }

    public void StartSession(string p_joinCode, string p_playerName = "DEFAULT_CLIENT")
    {
        try
        {
            Jacobs.Core.LobbyManager.Singleton.StartClient(p_joinCode, p_playerName);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}