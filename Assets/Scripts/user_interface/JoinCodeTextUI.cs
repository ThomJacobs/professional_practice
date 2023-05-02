using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))] public class JoinCodeTextUI : MonoBehaviour
{
    //Attributes:
    private TMPro.TextMeshProUGUI m_textBox = null;

    //Methods:
    private void Awake()
    {
        m_textBox = GetComponent<TMPro.TextMeshProUGUI>();
        UnityEngine.Assertions.Assert.IsNotNull<TMPro.TextMeshProUGUI>(m_textBox);

        m_textBox.text = Jacobs.Core.LobbyManager.Singleton.JoinCode;
    }
}
