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
        public NetworkList<ClientData> m_connectedClients = null;
        public UnityEngine.Events.UnityEvent OnValueChange = new UnityEngine.Events.UnityEvent();
        private Dictionary<ulong, int> m_clientToIndex = null;
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
        public NetworkList<ClientData> RegisteredClients
        {
            get => m_connectedClients;
        }

        //Methods:
        private void Awake()
        { 
            //We do not want our player information to be lost when a new scene is loaded.
            DontDestroyOnLoad(gameObject);

            //Initialise network list and dictionary.
            m_connectedClients = new NetworkList<ClientData>();
            m_clientToIndex = new Dictionary<ulong, int>();
        }

        private void OnValueChanged(NetworkListEvent<ClientData> p_client)
        {
            OnValueChange.Invoke();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("[UsernameManager] Number of clients registered: " + m_connectedClients.Count);

            m_connectedClients.OnListChanged += OnValueChanged;

            if (!IsServer) { return; }

            //Setup network manager.
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedCallback;

            foreach (ulong client in NetworkManager.ConnectedClientsIds)
            {
                m_connectedClients.Add(new ClientData { m_clientID = client, m_username = DEFAULT_USERNAME + client.ToString() });
                m_clientToIndex[client] = m_connectedClients.Count-1;
            }
        }

        public void SetClientUsername(ulong p_clientID, string p_username)
        {
            if(!m_clientToIndex.ContainsKey(p_clientID)) { return; }

            m_connectedClients[m_clientToIndex[p_clientID]] = new ClientData { m_clientID = p_clientID, m_username = p_username };
        }

        public ClientData GetClient(ulong p_clientID)
        {
            //Client has not been added to the list of connected clients.
            if(!m_clientToIndex.ContainsKey(p_clientID)) { Debug.LogError("[UsernameManager] Client with ID: " + p_clientID + " does not exist in structure."); return default; }

            return m_connectedClients[m_clientToIndex[p_clientID]];
        }

        public ClientData this[ulong p_clientID] => m_clientToIndex.ContainsKey(p_clientID) ? m_connectedClients[m_clientToIndex[p_clientID]] : default;

        private void ClientConnectedCallback(ulong p_clientID)
        {
            //If the game is setup correctly, it should be very unlikely for the same client to be added twice. (since they are removed during client disconnection). 
            if(m_clientToIndex.ContainsKey(p_clientID)) { return; }

            //Initialise and add data about the client to the dictionary.
            m_connectedClients.Add(new ClientData { m_clientID = p_clientID, m_username = DEFAULT_USERNAME + p_clientID.ToString() });
            m_clientToIndex[p_clientID] = m_connectedClients.Count-1;
        }

        private void ClientDisconnectedCallback(ulong p_clientID)
        {
            //If the client hasn't been added to the dictionary, then there is nothing to remove.
            if(!m_clientToIndex.ContainsKey(p_clientID)) { return; }

            //Remove the client.
            m_connectedClients.RemoveAt(m_clientToIndex[p_clientID]);
            m_clientToIndex.Remove(p_clientID);
        }

        public override void OnNetworkDespawn()
        {
            if(!IsHost) { return; }

            m_connectedClients.OnListChanged -= OnValueChanged;
            m_connectedClients.Clear();
            m_clientToIndex.Clear();

            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UsernameManager))]
    public class UsernameManagerEditor : UnityEditor.Editor
    {
        //Attributes:
        UsernameManager m_self = null; 

        //Methods:
        private void OnEnable()
        {
            m_self = (UsernameManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(m_self.m_connectedClients != null && UnityEngine.GUILayout.Button("Debug Users"))
            {
                for(int i = 0; i < m_self.m_connectedClients.Count; i++)
                {
                    Debug.Log("[UsernameManager] Client: " + m_self.m_connectedClients[i].ClientID);
                }
            }
        }
    }
#endif
}