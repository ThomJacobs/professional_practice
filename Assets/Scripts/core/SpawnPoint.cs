using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Underground.Spawning
{
    public class SpawnPoint : MonoBehaviour
    {
        //Attributes:


        //Properties:
        public virtual Vector3 Position
        {
            get => transform.position;
        }
    }
}