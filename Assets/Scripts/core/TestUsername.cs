using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUsername : Unity.Netcode.NetworkBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI m_textBox = null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_textBox = GetComponent<TMPro.TextMeshProUGUI>();
        m_textBox.text = "";

        for(int i = 0; i < Jacobs.Core.UsernameManager.Singleton.RegisteredClients.Count; i++)
        {
            m_textBox.text += Jacobs.Core.UsernameManager.Singleton.RegisteredClients[i].Username + "\n";
        }

        Jacobs.Core.UsernameManager.Singleton.OnValueChange.AddListener(OnValueChanged);
    }

    public void OnValueChanged()
    {
        m_textBox.text = "";

        for (int i = 0; i < Jacobs.Core.UsernameManager.Singleton.RegisteredClients.Count; i++)
        {
            m_textBox.text += Jacobs.Core.UsernameManager.Singleton.RegisteredClients[i].Username + "\n";
        }
    }
}
