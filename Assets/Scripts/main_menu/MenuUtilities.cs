using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jacobs.UI
{
    public sealed class MenuUtilities : Unity.Netcode.NetworkBehaviour
    {
        public void LoadScene(string p_name)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(p_name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public void StartRelay(string p_sceneName)
        {
            Core.RelayManager.Singleton.HostServer().ContinueWith(result => RelayLoadScene(p_sceneName));
        }

        public void RelayLoadScene(string p_sceneName)
        {
            Debug.Log(Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene(p_sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single).ToString());
        }

        public void JoinRelay(string p_joinCode)
        {
            Core.RelayManager.Singleton.JoinServer(p_joinCode).ContinueWith(result => Debug.Log("Success"));
        }
    }
}