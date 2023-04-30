using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

namespace Jacobs.Lobby
{
    /**
     * A UI script designed to handle and display the UI elements/components which present the player's connected to
     * a lobby, in addition to their ready status/state.
     * 
     * @owner Thomas Jacobs.
     * @client Alarming Ladder.
     */
    public sealed class PlayerReadyStatus : MonoBehaviour
    {
        //Attributes:
        [Header("Colour Settings")]
        [SerializeField] private Color m_readyColour = Color.green;
        [SerializeField] private Color m_waitColour = Color.red;

        [Header("Component Settings")]
        [SerializeField] private TextMeshProUGUI m_usernameTextBox = null;
        [SerializeField] private RawImage m_readyUpImage = null;

        private KeyCode m_actionKey = KeyCode.Space;

        private bool m_isReady = false;

        //Properties:
        public bool IsReady
        {
            //A bool could be cached to keep direct track of a player's status. (Memory vs Processing).
            get => m_isReady;

            set
            {
                m_isReady = value;

                //If the ready up (UI) image has been initialised, update it's colour accordingly.
                if (m_readyUpImage == null) { return; }
                m_readyUpImage.color = m_isReady ? m_readyColour : m_waitColour;
            }
        }

        public string Username
        {
            set
            {
                if(m_usernameTextBox == null) { return; }
                m_usernameTextBox.text = value;
            }
        }

        //Methods:
        private void Awake()
        {
            //The components must not be null!
            UnityEngine.Assertions.Assert.IsNotNull<TextMeshProUGUI>(m_usernameTextBox);
            UnityEngine.Assertions.Assert.IsNotNull<RawImage>(m_readyUpImage);
        }

        private void OnValidate()
        {
            if(m_readyUpImage == null) { return; }

            //If the ready up image has been initialised, set it's default/wait colour.
            m_readyUpImage.color = m_waitColour;
        }
    }
}