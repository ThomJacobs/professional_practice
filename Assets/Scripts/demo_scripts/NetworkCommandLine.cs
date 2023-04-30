using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Jacobs.Core
{
    /**
     * Based on Unity's documentation: https://docs-multiplayer.unity3d.com/netcode/current/tutorials/command-line-helper.
     */
    public sealed class NetworkCommandLine : MonoSingleton<NetworkCommandLine>
    {
        //Attributes:
        private NetworkManager m_networkManager;

        //Methods:
        private void Awake()
        {
            m_networkManager = NetworkManager.Singleton;

            if (Application.isEditor) return;
        }

        private Dictionary<string, string> GetCommandLineArgs()
        {
            Dictionary<string, string> argumentsDictionary = new Dictionary<string, string>();

            string[] arguments = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; ++i)
            {
                string argument = arguments[i].ToLower();

                if (!argument.StartsWith("-")) continue;

                string value = i < arguments.Length - 1 ? arguments[i + 1].ToLower() : null;
                argumentsDictionary.Add(argument, value);
            }

            return argumentsDictionary;
        }
    }
}