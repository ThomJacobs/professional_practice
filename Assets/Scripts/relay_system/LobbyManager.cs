using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using UnityEditor;
using System.Collections.Generic;

//https://docs-multiplayer.unity3d.com/netcode/0.1.0/learn/dapper/dapper-playernames/index.html
public struct PlayerData
{
    public string m_username;
}

[System.Serializable]
public class ConnectionPayload
{
    public string m_joinCode = string.Empty;
    public string m_playerName = string.Empty;
}

namespace Jacobs.Core
{
    /**
     * The relay manager utilises the Unity Relay Transport Protocol to allocate and establish a connection to a relay server.
     * In addition, the manager can also connect clients to a hosted session that has been activated by another client/host.
     * The data structure is intended for peer-to-peer networking where Unity relay allows easy setup over a Wide Area Network.
     * 
     * @client Alarming Ladder.
     * @owner Thomas Jacobs.
     * @date 20/04/23.
     */
    [RequireComponent(typeof(NetworkManager))] public sealed class LobbyManager : MonoSingleton<LobbyManager>
    {
        //Attributes:
        [Header("Relay Settings")]
        [SerializeField] private string m_environment = "production";
        [SerializeField] private int m_requestedConnections = 10;
        private RelayConnectionData m_hostData;

        [Header("Game Settings")]
#if UNITY_EDITOR
        public SceneAsset m_loadScene = null;
#endif

        private string m_loadSceneName = string.Empty;
        private const int MAXIMUM_ALLOWED_CONNECTIONS = 10;
        private const int MINIMUM_ALLOWED_CONNECTIONS = 2;
        private const UnityTransport.ProtocolType PROTOCOL_TYPE = UnityTransport.ProtocolType.RelayUnityTransport;
        private NetworkManager m_networkManager = null;
        private UsernameManager m_usernameManager = null;

        //Properties:
        public UnityTransport Transport => NetworkManager.Singleton.GetComponent<UnityTransport>();
        public bool IsEnabled => Transport != null && Transport.Protocol == PROTOCOL_TYPE;
        public string JoinCode => m_hostData.m_joinCode;

        /*
         * When using the relay manager to create a networked session it is recommneded that this instance is used when
         * a reference to the network manager is needed. However, the relay manager will set it's network manager instance 
         * as the singleton instance.
         * 
         * @return The relay manager instance setup for relay network.
         */
        public NetworkManager NetworkManager => m_networkManager;
        public UsernameManager UsernameManager => m_usernameManager;

        private void Awake()
        {
            //Initialise the network manager singleton.
            m_networkManager = GetComponent<NetworkManager>();
            m_networkManager.SetSingleton();

            m_usernameManager = UsernameManager.Singleton;
        }

        /**
         *  Executed whenever a value in the component's inspector is modified.
         */
        private void OnValidate()
        {
            //Check the developer hasn't tried to set the number of requested connections above or below the minimum and maximum thresholds.
            if (m_requestedConnections > MINIMUM_ALLOWED_CONNECTIONS && m_requestedConnections < MAXIMUM_ALLOWED_CONNECTIONS) { return; }

            m_requestedConnections = Mathf.Clamp(m_requestedConnections, MINIMUM_ALLOWED_CONNECTIONS, MAXIMUM_ALLOWED_CONNECTIONS);
            Debug.LogWarning("Relay Manager: The requested number of connections must be between " + MINIMUM_ALLOWED_CONNECTIONS + " and " + MAXIMUM_ALLOWED_CONNECTIONS);

#if UNITY_EDITOR
            if(m_loadScene) m_loadSceneName = m_loadScene.name;
#endif
        }

        /**
         *  Initialises Untity's services and signs the player in anonymously if they are currently signed out.
         *  
         *  @param p_initialisationOptions: Passed over to 'Unity Services' to aid the initialisation setup.
         *  @return True when the player has been successfully signed in, False when an exception has occured (Check Unity Log Files).
         */
        public async Task<bool> AnonymouseSignIn(InitializationOptions p_initialisationOptions)
        {
            try
            {
                //If unity services are uninitallised, initialise unity services.
                if (UnityServices.State == ServicesInitializationState.Uninitialized) { await UnityServices.InitializeAsync(p_initialisationOptions); }

                //If the player is already signed in and the session is not expired return true.
                if(AuthenticationService.Instance.IsSignedIn && !AuthenticationService.Instance.IsExpired) { return true; }

                //Sign in player anonymously.
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                return true;
            }
            catch(AuthenticationException exception)
            {
                //If an exception is caught relay it's message back through Unity's editor system.
                Debug.LogException(exception);
                return false;
            }

            catch(RequestFailedException exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        /**
         * Authenticates the player and providing authentication is successful, allocates a relay sever in accordance
         * to the number of connections requested/assigned in the inspector.
         * 
         * @return Information about the relay host.
         */
        public async void StartHost(string p_playerName = "DEFAULT_HOST")
        {
            //Authenticate the player first. (If the player is not already signed in, sign them in anonymously.
            await AnonymouseSignIn(new InitializationOptions().SetEnvironmentName(m_environment));

            //Allocate an available relay server connection.
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(m_requestedConnections);

            RelayConnectionData relay_host_data = new RelayConnectionData(allocation, await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId), RelayConnectionType.Host);
            
            //Pass relay server data over to Unity Transport.
            Transport.SetRelayServerData(relay_host_data.m_ipv4Address, relay_host_data.m_port, relay_host_data.m_allocationIDBytes, relay_host_data.m_key, relay_host_data.m_connectionData);

            //Add host to list of clients.
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCallback;

            NetworkManager.Singleton.StartHost();

            m_usernameManager.SetClientUsername(m_networkManager.LocalClientId, p_playerName);

            Debug.Log("Relay generated join code: " + relay_host_data.m_joinCode);

            //Setup and load the main level.
            NetworkManager.Singleton.SceneManager.LoadScene("lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);

            m_hostData = relay_host_data;
        }

        //https://docs-multiplayer.unity3d.com/netcode/current/basics/connection-approval/index.html
        private void ApprovalCallback(NetworkManager.ConnectionApprovalRequest p_request, NetworkManager.ConnectionApprovalResponse p_response)
        {
            ulong clientID = p_request.ClientNetworkId;
            ConnectionPayload payload = JsonUtility.FromJson<ConnectionPayload>(System.Text.Encoding.ASCII.GetString(p_request.Payload));

            m_usernameManager.SetClientUsername(clientID, payload.m_playerName+"_"+clientID.ToString());

            p_response.Approved = true;
            p_response.Pending = false;
        }

        /**
         * Providing the player has been authenticated, joins a session allocated by an external host.
         * 
         * @return Information about the relay network connection.
         */
        public async void StartClient(string p_joinCode, string p_playerName = "DEFAULT_CLIENT")
        {
            InitializationOptions options = new InitializationOptions().SetEnvironmentName(m_environment);
            await UnityServices.InitializeAsync(options);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(p_joinCode);

            RelayJoinData relayJoinData = new RelayJoinData
            {
                key = allocation.Key,
                port = (ushort)allocation.RelayServer.Port,
                allocationID = allocation.AllocationId,
                allocationIDBtytes = allocation.AllocationIdBytes,
                connectionData = allocation.ConnectionData,
                hostConnectionData = allocation.HostConnectionData,
                ipv4Address = allocation.RelayServer.IpV4,
                joinCode = p_joinCode,
            };

            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                m_playerName = p_playerName,
                m_joinCode = p_joinCode
            });

            //Send information for the server to validate.
            byte[] payloadBytes = System.Text.Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            //Start client.
            Transport.SetRelayServerData(relayJoinData.ipv4Address, relayJoinData.port, relayJoinData.allocationIDBtytes, relayJoinData.key, relayJoinData.connectionData, relayJoinData.hostConnectionData);
            NetworkManager.Singleton.StartClient();

            Debug.Log("Client joined game with join code: " + p_joinCode);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LobbyManager))]
    public class RelayManagerEditor : Editor
    {
        //Attributes:
        private LobbyManager m_self = null;

        //Methods:
        private void OnEnable() => m_self = (LobbyManager)target;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Developer Settings", EditorStyles.boldLabel);

            if(GUILayout.Button("Begin Host") && Application.isPlaying) { m_self.StartHost(); }
        }
    }
#endif
}