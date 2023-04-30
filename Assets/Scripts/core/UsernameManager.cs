using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

namespace Jacobs.Core
{
    public sealed class UsernameManager : Core.NetworkSingleton<UsernameManager>
    {
        //Attributes:
        public NetworkDictionary<ulong, ClientData> m_connectedClients;
        public UnityEngine.Events.UnityEvent OnValueChange = new UnityEngine.Events.UnityEvent();
        private const string DEFAULT_USERNAME = "DEFAULT_USER_";

        //Structures:
        public struct ClientData : System.IEquatable<ClientData>, INetworkSerializable
        {
            //Attributes:
            public FixedString64Bytes m_username;
            public ulong m_clientID;

            //Properties:
            public string Username
            {
                get => m_username.Value;
                set => m_username = value;
            }

            public ulong ClientID => m_clientID;

            public bool Equals(ClientData other)
            {
                return Username == other.Username && ClientID == other.ClientID; 
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue<FixedString64Bytes>(ref m_username);
                serializer.SerializeValue<ulong>(ref m_clientID);
            }
        }

        //Properties:
        public IEnumerator<ulong> RegisteredClients
        {
            get => m_connectedClients.Keys.GetEnumerator();
        }

        //Methods:
        private void Awake()
        { 
            //We do not want our player information to be lost when a new scene is loaded.
            DontDestroyOnLoad(gameObject);

            //Initialise network list and dictionary.
            m_connectedClients = new NetworkDictionary<ulong, ClientData>();
        }

        private void OnValueChanged(NetworkDictionaryEvent<ulong, ClientData> p_client)
        {
            OnValueChange.Invoke();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("[UsernameManager] Number of clients registered: " + m_connectedClients.Count);

            m_connectedClients.OnDictionaryChanged += OnValueChanged;

            if (!IsServer) { return; }

            //Setup network manager.
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedCallback;

            foreach (ulong client in NetworkManager.ConnectedClientsIds)
            {
                m_connectedClients.Add(client, new ClientData { m_clientID = client, m_username = DEFAULT_USERNAME + client.ToString() });
            }
        }

        public void SetClientUsername(ulong p_clientID, string p_username)
        {
            if(!m_connectedClients.ContainsKey(p_clientID)) { return; }

            m_connectedClients[p_clientID] = new ClientData { m_clientID = p_clientID, m_username = p_username };
        }

        public ClientData GetClient(ulong p_clientID)
        {
            //Client has not been added to the list of connected clients.
            if(!m_connectedClients.ContainsKey(p_clientID)) { Debug.LogError("[UsernameManager] Client with ID: " + p_clientID + " does not exist in structure."); return default; }

            return m_connectedClients[p_clientID];
        }

        public ClientData this[ulong p_clientID] => m_connectedClients.ContainsKey(p_clientID) ? m_connectedClients[p_clientID] : default;

        private void ClientConnectedCallback(ulong p_clientID)
        {
            //If the game is setup correctly, it should be very unlikely for the same client to be added twice. (since they are removed during client disconnection). 
            if(m_connectedClients.ContainsKey(p_clientID)) { return; }

            //Initialise and add data about the client to the dictionary.
            m_connectedClients.Add(p_clientID, new ClientData { m_clientID = p_clientID, m_username = DEFAULT_USERNAME + p_clientID.ToString() });
        }

        private void ClientDisconnectedCallback(ulong p_clientID)
        {
            //If the client hasn't been added to the dictionary, then there is nothing to remove.
            if(!m_connectedClients.ContainsKey(p_clientID)) { return; }

            //Remove the client.
            m_connectedClients.Remove(p_clientID);
        }

        public override void OnNetworkDespawn()
        {
            if(!IsHost) { return; }

            m_connectedClients.OnDictionaryChanged -= OnValueChanged;
            m_connectedClients.Clear();

            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}