using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Jacobs.Core
{
    [System.Serializable]
    struct PoolRequest
    {
        public GameObject m_prefabrication;
        public uint m_size;
    }

    public sealed class ObjectPool : Core.NetworkSingleton<ObjectPool>
    {
        //Attributes:
        [SerializeField] private List<PoolRequest> m_poolRequests = new List<PoolRequest>();
        [SerializeField] private bool m_initialiseOnAwake = false;
        private Dictionary<GameObject, Queue<GameObject>> m_objectPools = new Dictionary<GameObject, Queue<GameObject>>();

        //Methods:
        private void Awake()
        {
            if(m_initialiseOnAwake) { Initialise(); }
        }

        public void Initialise()
        {
            for(int i = 0; i < m_poolRequests.Count; i++)
            {
                //If object pool does not already exist, intialise a new pool.
                if(m_poolRequests[i].m_size > 0 && !m_objectPools.ContainsKey(m_poolRequests[i].m_prefabrication)) { m_objectPools.Add(m_poolRequests[i].m_prefabrication, new Queue<GameObject>()); } 

                for(int j = 0; j < m_poolRequests[i].m_size; j++)
                {
                    m_objectPools[m_poolRequests[i].m_prefabrication].Enqueue(m_poolRequests[i].m_prefabrication);
                }
            }
        }

        private void AddPrefab(GameObject p_prefab)
        {
            GameObject clone = Instantiate(p_prefab);
            m_objectPools[p_prefab].Enqueue(clone);
        }
    }
}