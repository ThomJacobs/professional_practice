using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

//https://docs-multiplayer.unity3d.com/netcode/0.1.0/learn/dapper/dapper-playernames/index.html

namespace Jacobs.Lobby
{
    public struct LobbyPlayerState : INetworkSerializable, System.IEquatable<LobbyPlayerState>
    {
        //Attributes:
        public ulong m_clientID;
        public bool m_isReady;

        //Constructor:
        public LobbyPlayerState(ulong p_clientID, bool p_isReady = false)
        {
            m_clientID = p_clientID;
            m_isReady = p_isReady;
        }

        public bool Equals(LobbyPlayerState p_other)
        {
            return p_other.m_clientID == m_clientID && p_other.m_isReady == m_isReady;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> p_serializer) where T : IReaderWriter
        {
            p_serializer.SerializeValue(ref m_clientID);
            p_serializer.SerializeValue(ref m_isReady);
        }
    }
    //https://www.youtube.com/watch?v=sBR0oJJjx6Q&t=166s
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class ReadyController : NetworkBehaviour
    {
        //Attributes:
        [SerializeField] private PlayerReadyStatus m_playerReadyStatusPrefab = null;
        [SerializeField] private KeyCode m_actionKey = KeyCode.Return;
        [SerializeField] private PlayerReadyStatus[] m_playerCards;
        private NetworkList<LobbyPlayerState> m_activeClients = null;

        //private NetworkList<LobbyPlayerState> m_lobbyPlayers = new NetworkList<LobbyPlayerState>();

        //Properties:
        bool CanStartGame
        {
            get
            {
                for (int i = 0; i < m_activeClients.Count; i++)
                {
                    if (!m_activeClients[i].m_isReady) { return false; }
                }
                return true;
            }
        }

        //Methods:
        private void Awake()
        {
            m_activeClients = new NetworkList<LobbyPlayerState>();
            //m_activeClients.Initialize(this);
        }

        public void TryLoadScene(string p_name)
        {
            if (!IsServer || !CanStartGame) { return; }
            NetworkManager.SceneManager.LoadScene(p_name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                for(int i = 0; i < m_activeClients.Count; i++)
                {
                    if(m_activeClients[i].m_clientID == NetworkManager.LocalClientId) 
                    { 
                        bool ready = m_activeClients[i].m_isReady;
                        ulong ID = m_activeClients[i].m_clientID;
                        if (IsServer) m_activeClients[i] = new LobbyPlayerState(ID, !ready);
                        else if (IsClient) UpdateCardServerRpc(i, new LobbyPlayerState(ID, !ready));
                    }
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();


            if(IsServer)
            {
                NetworkManager.OnClientConnectedCallback += Callback;
                m_activeClients.OnListChanged += OnChange;

                foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    m_activeClients.Add(new LobbyPlayerState(client, false));
                }
            }

            else if(IsClient && !IsHost)
            {
                m_activeClients.OnListChanged += OnChange;

                OnChange(new NetworkListEvent<LobbyPlayerState>());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCardServerRpc(int p_index, LobbyPlayerState p_state)
        {
            if(p_index >= m_activeClients.Count || p_index < 0) { return; }

            m_activeClients[p_index] = p_state;
        }

        private void OnChange(NetworkListEvent<LobbyPlayerState> p_state)
        {
            for(int i = 0; i < m_playerCards.Length; i++)
            {
                if(i >= m_activeClients.Count)
                {
                    m_playerCards[i].gameObject.SetActive(false);
                }
                else
                {
                    m_playerCards[i].gameObject.SetActive(true);
                    m_playerCards[i].Username = Core.UsernameManager.Singleton.GetClient(m_activeClients[i].m_clientID).Username;
                    m_playerCards[i].IsReady = m_activeClients[i].m_isReady;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(IsServer) NetworkManager.OnClientConnectedCallback -= Callback;
        }

        private void Callback(ulong p_ID)
        {
            m_activeClients.Add(new LobbyPlayerState(p_ID));
        }
    }
}