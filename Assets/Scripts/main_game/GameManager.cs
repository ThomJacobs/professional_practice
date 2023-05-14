using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Underground.Core
{
    /**
     * Responsible for setting up the start of the game and ending a game/session when the win/lose quota
     * is met.
     */
    public class GameManager : NetworkBehaviour
    {
        //Attributes:
        [SerializeField] private Spawning.PlayerSpawner m_PlayerSpawner = null;
        private Dictionary<ulong, PlayerState> m_PlayerStates = new Dictionary<ulong, PlayerState>();

        //Methods:

        /**
         * Called at the start of the scene.
         */
        private void Awake()
        {
            //The player spawner must be specified an instance active in the scene.
            if(m_PlayerSpawner == null)
            {
                Debug.LogError("[GameManager] The player spawner has not been initialised/setup.");
                Destroy(this);
                return;
            }

            //The spawner should be setup to not spawn on awake; however, it doesn't hurt to be sure.
            m_PlayerSpawner.m_SpawnOnAwake = false;
        }

        [ServerRpc]
        private void NotifyServerRpc(ulong p_ClientID, bool p_AliveValue)
        {
            Debug.Log("Player has changed state");
            OnPlayerStateChange(p_ClientID, p_AliveValue);
        }

        private void OnPlayerStateChange(ulong p_ClientID, bool p_AliveValue)
        {
            if(IsClient) { NotifyServerRpc(p_ClientID, p_AliveValue); return; }


        }

        /**
         * Called when a client/host is spawned in the scene.
         */
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //Spawn the playable controller which will represent the client/host.
            NetworkObject networkObject = m_PlayerSpawner.Spawn();

            //Allocate and setup the client a 'player-state' component.
            if(IsClient) { return; }

            PlayerState state = networkObject.gameObject.AddComponent<PlayerState>();
            state.IsAlive = true;
            state.OnValueChange += OnPlayerStateChange;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}