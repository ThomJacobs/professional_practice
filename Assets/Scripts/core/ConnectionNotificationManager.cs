using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

namespace Jacobs.Core
{
    public enum ConnectionStatus
    {
        Disconnected = 0,
        Connected = 1
    }

    public sealed class ConnectionNotificationManager : Core.NetworkSingleton<ConnectionNotificationManager>
    {
        //Attributes:
        public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;
        public Dictionary<ulong, string> m_connectedClients = new Dictionary<ulong, string>();
        private const string DEFAULT_USERNAME = "PLAYER_";
        private static ConnectionNotificationManager singleton_instance = null;

        //Methods:
        public override void OnNetworkSpawn()
        {
            DontDestroyOnLoad(gameObject);

            //The network manager must be initialised before a networked session can begin.
            UnityEngine.Assertions.Assert.IsNotNull<NetworkManager>(NetworkManager.Singleton);

            //Assign callback methods.
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;

            OnClientConnectedCallback(NetworkManager.LocalClientId);
        }

        public override void OnDestroy()
        {
            if(NetworkManager.Singleton == null) { return; }

            //Remove callback methods.
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }

        private void OnClientConnectedCallback(ulong p_clientID)
        {
            //Add client to dictionary of connected clients.
            m_connectedClients.Add(p_clientID, DEFAULT_USERNAME + p_clientID.ToString());

            //Notify assigned callbacks.
            OnClientConnectionNotification?.Invoke(p_clientID, ConnectionStatus.Connected);
        }

        private void OnClientDisconnectedCallback(ulong p_clientID)
        {
            //Remove client to dictionary of connected clients.
            m_connectedClients.Remove(p_clientID);

            //Notify assigned callbacks.
            OnClientConnectionNotification?.Invoke(p_clientID, ConnectionStatus.Disconnected);
        }
    }
}