using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEditor;
using UnityEngine.Networking;

namespace Jacobs.Core
{
    [RequireComponent(typeof(TMPro.TextMeshPro))] public class PlayerTag : NetworkBehaviour
    {
        //Attributes:
        private TMPro.TextMeshPro m_textBox = null;
        private static readonly Color[] COLOUR_OPTIONS = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan };

        //Properties:
        public TMPro.TextMeshPro TextBox
        {
            get
            {
                //The text mesh component is null/unassigned.
                if (m_textBox == null) m_textBox = GetComponent<TMPro.TextMeshPro>();

                return m_textBox;
            }
        }

        public string Name
        {
            set => TextBox.text = value;

            get => TextBox.text;
        }

        public Color Colour
        {
            get => TextBox.color;
            set => TextBox.color = value;
        }

        //Methods:

        //Simple setup for the purposes of the demonstration only.
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //The server will setup the player then relay the information back to the clients to avoid any error.
            if(!IsServer) { return; }

            //Set a random colour.
            Colour = COLOUR_OPTIONS[Random.Range(default, COLOUR_OPTIONS.Length)];

            //Set name equal to the current number of connected clients.
            Name = "Player_" + (NetworkManager.Singleton.ConnectedClients.Count).ToString();

            //Update the clients.
            UpdateClientsClientRpc(Colour, Name);
        }

        [ClientRpc]
        private void UpdateClientsClientRpc(Color p_colour, string p_name)
        {
            Name = p_name;
            Colour = p_colour;
        }

        private void LateUpdate()
        {
            if (IsLocalPlayer || IsOwner) return;

            transform.LookAt(Camera.main.transform);
        }
    }
}