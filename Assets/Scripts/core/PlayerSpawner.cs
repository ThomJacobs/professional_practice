using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Underground.Spawning
{
    /*
     * The player spawner is a client side spawner which has the purpose of registering, initialising, and spawning a gameObject for a client to use when they load a scene
     * with a player spawner instance. The script also handles despawning the player object if it hasn't already been despawned and destroyed by external scripts when the player
     * spawner is despawned/destroyed.
     */
    public sealed class PlayerSpawner : NetworkBehaviour
    {
        //Attributes:
        [Header("Player Settings")]
        public GameObject m_prefabAsset = null;
        private NetworkObject m_spawnedObject = null;

        [Header("Spawn Settings")]
        public List<SpawnPoint> m_spawnPoints = null;

        /*
         * Called once when a gameObject is first created/initialised.
         */
        private void Awake()
        {
            //If the prefab asset is null/empty then the script has no gameObject to spawn for the player and will return.
            if (m_prefabAsset == null) { Destroy(this); return; }

            //Add the prefab to the network manager, ready for spawning.
            Jacobs.Core.LobbyManager.Singleton.NetworkManager.AddNetworkPrefab(m_prefabAsset);

            //Instantiate the gameObject and set it as inactive until the client is spawned.
            m_spawnedObject = Instantiate<NetworkObject>(m_prefabAsset.GetComponent<NetworkObject>());
            m_spawnedObject.gameObject.SetActive(false);
        }

        private SpawnPoint PopSpawnPoint()
        {
            //Generate a random spawn point to position the player on.
            SpawnPoint spawnPoint = m_spawnPoints[Random.Range(0, m_spawnPoints.Count)];

            //Remove the spawn point from the host/server instance.
            m_spawnPoints.Remove(spawnPoint);

            return spawnPoint;
        }

        [ServerRpc]
        private void SpawnServerRpc(ref NetworkObject p_networkObject, ulong p_clientOwner)
        {
#if UNITY_EDITOR
            if(m_spawnPoints.Count <= 0) { Debug.LogWarning("[PlayerSpawner] Spawn system has ran out of spawn points to spawn clients."); return; }
#endif
            //Generate a random spawn point to position the player on.
            p_networkObject.transform.position = PopSpawnPoint().Position;
            p_networkObject.Spawn();

            //Ensure the network object keeps the desired ownership.
            p_networkObject.ChangeOwnership(p_clientOwner);
        }

        /*
         * Called when an object is spawned on a network.
         */
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //The spawned object should not be destroyed by any external script.
            if (m_spawnedObject == null) { Debug.LogWarning("[PlayerSpawner] spawned object was not set to a prefab instance."); Destroy(this); return; };

            //Activate and spawn the player object.
            m_spawnedObject.gameObject.SetActive(true);

            //Send a message to the server/host instructing them to spawn the object.
            if (IsClient) { SpawnServerRpc(ref m_spawnedObject, m_spawnedObject.NetworkManager.LocalClientId); return; }

            //If this is a host/server instance, the player can be directly spawned from this instance.
            m_spawnedObject.transform.position = PopSpawnPoint().Position;
            m_spawnedObject.Spawn();
        }

        /*
         * Called when an object is 'despawned' from a network.
         */
        public override void OnNetworkDespawn()
        {
            //If the spawned object hasn't been destroyed, despawn and deactivate it.
            if (m_spawnedObject != null && m_spawnedObject.IsSpawned)
            {
                m_spawnedObject.gameObject.SetActive(false);
                m_spawnedObject.Despawn();
                m_spawnedObject = null;
            }

            m_prefabAsset = null;

            base.OnNetworkDespawn();
        }
    }
}