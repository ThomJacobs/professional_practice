using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Jacobs.Core
{
    [System.Serializable] struct SpawnRequest
    {
        //Attributes:
        [SerializeField] private NetworkObject m_networkObject;
        [SerializeField] private uint m_instances;

        //Properties:
        public NetworkObject NetworkObject => m_networkObject;
        public GameObject GameObject => m_networkObject != null ? m_networkObject.gameObject : default;
        public uint NumberOfInstances => m_instances;
    }
    
    /**
     * A network-pool is an extension of the more common (game) object pool which allocates the game-objects and components (data) required
     * by other scripts at the start of a game/session, instead of creating them during runtime which can affect runtime performance. In addition,
     * the structure also implements methods for spawning game-objects/components during a networked session.
     * 
     * @owner Thomas Jacobs.
     * @client Alarming Ladder.
     */
    [RequireComponent(typeof(NetworkObject))] public sealed class NetworkPool : Core.NetworkSingleton<NetworkPool>
    {
        //Attributes:
        [SerializeField] private List<SpawnRequest> m_spawnRequests = new List<SpawnRequest>();
        private Dictionary<NetworkObject, Queue<NetworkObject>> m_networkPools = new Dictionary<NetworkObject, Queue<NetworkObject>>();
        private static readonly Vector3 ORIGIN = Vector3.zero;

        //Methods:
        private void Awake()
        {
            //If no spawn requests have been assigned return from the awake method.
            if (m_spawnRequests.Count <= 0) return;

            //Allocate all requested network objects.
            for(int i = 0; i < m_spawnRequests.Count; i++)
            {
                //Skip any requests which have not been assigned correctly.
                if (m_spawnRequests[i].NetworkObject == null) 
                {
#if UNITY_EDITOR
                    Debug.LogWarning(gameObject.name + ": Spawn request at index ["+i+"] has not been assigned.");
#endif
                    continue; 
                }

                //If pool for network object isn't already allocated initialise a new queue.
                if(!m_networkPools.ContainsKey(m_spawnRequests[i].NetworkObject)) { m_networkPools.Add(m_spawnRequests[i].NetworkObject, new Queue<NetworkObject>()); }

                //Initialise network objects.
                for(int j = 0; j < m_spawnRequests[i].NumberOfInstances; j++)
                {
                    //Instantiate, name, position and disable game-object until it is requested during the game's runtime.
                    NetworkObject clone = GameObject.Instantiate<NetworkObject>(m_spawnRequests[i].NetworkObject);
                    clone.name = "pooled_object_" + m_spawnRequests[i].NetworkObject.name + "_" + j.ToString();
                    DisableObject(clone.gameObject);
                }
            }
        }

        private void EnableObject(GameObject p_object)
        {
            p_object.gameObject.SetActive(true);
        }

        private void DisableObject(GameObject p_object)
        {
            p_object.gameObject.SetActive(false);
            p_object.transform.position = ORIGIN;
        }

        public GameObject RequestSpawn(NetworkObject p_key, bool p_destroyWithScene = false)
        {
            if(!m_networkPools.ContainsKey(p_key) || m_networkPools[p_key].Count <= 0) { Debug.LogError(name + ": Object requested for spawn (" + p_key + ") has no instances left to spawn/allocate."); return default; }

            //Dequeue and spawn an available object from the object pools.
            NetworkObject target = m_networkPools[p_key].Dequeue();
            target.Spawn(p_destroyWithScene);
            return target.gameObject;
        }


#if UNITY_EDITOR
        [UnityEditor.MenuItem("Jacobs/NetworkPool")] private static void GetEditorInstance()
        {
            //(The call of the singleton property will invoke the initialisation of the network-pool if it hasn't already beeen initialised).
            UnityEditor.Selection.activeGameObject = Singleton.gameObject;
        }
#endif
    }
}