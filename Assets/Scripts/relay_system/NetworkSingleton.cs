using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Jacobs.Core
{
    /**
     *  Manages static instances where the defined generic type inherits Unity's component class.
     * 
     *  @owner Thomas Jacobs.
     *  @date 3/4/23.
     */
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSingleton<DerivedType> : NetworkBehaviour where DerivedType : Component
    {
        //Attributes:
        private static DerivedType singleton_instance;

        //Methods:
        /**
         *  @return Singleton instance of the defined, generic type.
         */
        public static DerivedType Singleton
        {
            get
            {
                if (singleton_instance != null) return singleton_instance;

                DerivedType[] obj = FindObjectsOfType<DerivedType>();

                if (obj.Length > 0)
                {
                    if (obj.Length > 1) { Debug.LogError("There is more than one " + typeof(DerivedType).Name + " instance in the scene."); }
                    singleton_instance = obj[0]; return singleton_instance;
                }

                //Initialise and spawn object.
                singleton_instance = new GameObject(typeof(DerivedType).Name + "_Singleton").AddComponent<DerivedType>();

                return singleton_instance;
            }
        }
    }
}