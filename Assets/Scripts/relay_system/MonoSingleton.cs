using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Manages static instances where the defined generic type inherits Unity's component class.
 * 
 *  @owner Thomas Jacobs.
 *  @date 3/4/23.
 */
public class MonoSingleton<DerivedType> : MonoBehaviour where DerivedType : Component
{
    //Attributes:
    private static DerivedType singleton_instance;

    //Properties:

    /**
     *  @return Singleton instance of the defined, generic type.
     */
    public static DerivedType Singleton
    {
        get
        {
            if (singleton_instance != null) return singleton_instance;

            DerivedType[] obj = FindObjectsOfType<DerivedType>();

            if(obj.Length > 0) 
            {
                if (obj.Length > 1) { Debug.LogError("There is more than one " + typeof(DerivedType).Name + " instance in the scene."); }
                singleton_instance = obj[0]; return singleton_instance; 
            }

            singleton_instance = new GameObject(typeof(DerivedType).Name + "_Singleton").AddComponent<DerivedType>();
            return singleton_instance;
        }
    }

    //Methods:

    /*
     * Assigns the specified instance as the singleton instance.
     * 
     * @p_singletonInstance: The instance that will be as the singleton instance.
     */
    public static void SetSingleton(DerivedType p_singletonInstance)
    {
        UnityEngine.Assertions.Assert.IsNotNull<DerivedType>(p_singletonInstance);
        singleton_instance = p_singletonInstance;
    }
}
