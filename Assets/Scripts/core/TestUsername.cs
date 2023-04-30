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

        IEnumerator<ulong> clients = Jacobs.Core.UsernameManager.Singleton.RegisteredClients;

        while(clients.MoveNext())
        {
            m_textBox.text += clients.Current.ToString()+ "\n";
        }

        Jacobs.Core.UsernameManager.Singleton.OnValueChange.AddListener(OnValueChanged);
    }

    public void OnValueChanged()
    {
        m_textBox.text = "";

        IEnumerator<ulong> clients = Jacobs.Core.UsernameManager.Singleton.RegisteredClients;

        while (clients.MoveNext())
        {
            m_textBox.text += clients.Current.ToString() + "\n";
        }
    }
}
