using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;

public class PlayerTest : NetworkBehaviour
{
    //Attributes:
    private string playerName;
    [SerializeField] private TMPro.TextMeshPro textbox;

    //Methods:
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerName = GetComponent<NetworkObject>().NetworkObjectId.ToString();
        textbox.text = name;
        transform.position += new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
    }

    private void OnChangeName(string name)
    {
        textbox.text = name;
    }
}
