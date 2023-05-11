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
    public sealed class PlayerSpawner : NetworkBehaviour, INetworkPrefabInstanceHandler
    {
        //Attributes:
        [Header("Player Settings")]
        [SerializeField] private GameObject m_PrefabAsset = null;

        [Header("Spawn Settings")]
        public List<SpawnPoint> m_SpawnPoints = null;

        private GameObject m_SpawnedPrefab = null;
        private NetworkObject m_SpawnedNetworkObject = null;

        /*
         * Called once when a gameObject is first created/initialised.
         */
        private void Awake()
        {
            //The prefab asset must be setup, otherwise the client will have no 'playable' game-object to represent them in a scene.
            UnityEngine.Assertions.Assert.IsNotNull<GameObject>(m_PrefabAsset);
            UnityEngine.Assertions.Assert.IsTrue(m_SpawnPoints.Count > 0);

            //Instantiate the gameObject and set it as inactive until the client is spawned.
            m_SpawnedNetworkObject = Instantiate<NetworkObject>(m_PrefabAsset.GetComponent<NetworkObject>());
            m_SpawnedNetworkObject.gameObject.SetActive(false);
            m_SpawnedPrefab = m_SpawnedNetworkObject.gameObject;
        }

        private SpawnPoint PopSpawnPoint()
        {
            //Generate a random spawn point to position the player on.
            SpawnPoint spawnPoint = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];

            //Remove the spawn point from the host/server instance. (Retain one spawn point to exclude the chance of running out of spawn points).
            if (m_SpawnPoints.Count > 1) { m_SpawnPoints.Remove(spawnPoint); }

            return spawnPoint;
        }

        private void Spawn()
        {
            //Only the host client can spawn a player. If the player prefab is null or has already spawned the program will exit the function.
            if(!IsServer || m_SpawnedPrefab == null || m_SpawnedNetworkObject.IsSpawned) { return; }

            //Activate and spawn prefab.
            m_SpawnedPrefab.SetActive(true);
            m_SpawnedPrefab.transform.position = PopSpawnPoint().transform.position;
            m_SpawnedNetworkObject.Spawn();
        }

        /*
         * Called when an object is spawned on a network.
         */
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.PrefabHandler.AddHandler(m_PrefabAsset, this);

            //The spawned object should not be destroyed by any external script.
            if (m_PrefabAsset == null) { Debug.LogWarning("[PlayerSpawner] spawned object was not set to a prefab instance."); Destroy(this); return; };

            if(!IsServer) { return; }

            //If this is a host/server instance, the player can be directly spawned from this instance.
            Spawn();
        }

        /*
         * Called when an object is 'despawned' from a network.
         */
        public override void OnNetworkDespawn()
        {
            //If the spawned object hasn't been destroyed, despawn and deactivate it.
            if (m_SpawnedNetworkObject != null && m_SpawnedNetworkObject.IsSpawned)
            {
                m_SpawnedNetworkObject.gameObject.SetActive(false);
                m_SpawnedNetworkObject.Despawn();
                m_SpawnedNetworkObject = null;
            }

            m_PrefabAsset = null;

            base.OnNetworkDespawn();
        }

        public NetworkObject Instantiate(ulong p_ClientID, Vector3 p_Position, Quaternion p_Rotation)
        {
            //Activate the prefab and translate it according to the values of the parameters.
            m_SpawnedPrefab.SetActive(true);
            m_PrefabAsset.transform.position = p_Position;
            m_PrefabAsset.transform.rotation = p_Rotation;
            return m_SpawnedNetworkObject;
        }

        public void Destroy(NetworkObject networkObject)
        {
            //Deactivate the prefab.
            m_SpawnedPrefab.SetActive(false);
        }
    }
}