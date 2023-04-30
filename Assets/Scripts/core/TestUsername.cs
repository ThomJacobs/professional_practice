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

        IEnumerator<Jacobs.Core.UsernameManager.ClientData> enumerable = Jacobs.Core.UsernameManager.Singleton.RegisteredClients;

        while (enumerable.MoveNext())
        {
            m_textBox.text += enumerable.Current.Username +"\n";
        }
    }
}
