using UnityEngine;
using Unity.Netcode;

namespace Jacobs.Core
{
    /*
     * A basic movement controller which uses all three components of a three vector to move a game-object around
     * in world space. More importantly, the controller is specifically setup as an example for handling peer-to-peer
     * client-side (authority) physics and transformation.
     * 
     * @owner Thomas Jacobs.
     * @client Alarming Ladder.
     */
    [RequireComponent(typeof(Rigidbody))] public sealed class PlayerController : NetworkBehaviour
    {
        //Attributes:
        [Header("Movement Settings")]
        public float m_moveSpeed = 1.0f;
        public KeyCode m_jumpKey = KeyCode.Space;

        [Header("Rotation Settings")]
        public float m_rotateSensitivity = 1.0f;
        public bool m_isInverted = false;

        [Header("Jump Settings")]
        [SerializeField] private float m_jumpForce = 1.0f;
        [SerializeField] private float m_raycastLength = 1.0f;
        private Rigidbody m_rigidbody = null;
        private bool m_isJumpRequested = false;

        //Properties:
        private static float HorizontalAxis => Input.GetAxis("Horizontal");
        private static float VerticalAxis => Input.GetAxis("Vertical");
        private float MouseX => m_isInverted ? Input.GetAxis("Mouse X") : Input.GetAxis("Mouse X") * -1;
        private bool IsGrounded => Physics.Raycast(transform.position, -transform.up, m_raycastLength * transform.localScale.magnitude);

        //Methods:
        /**
         * Called when a new player controller is initialised (spawned) by the network manager.
         */
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //Place the player at a random locaton.
            m_rigidbody = GetComponent<Rigidbody>();

            if (!IsServer) { return; }
            transform.position = new Vector3(Random.Range(-2, 2), 4.0f, Random.Range(-2, 2));

            //Only activate the camera for the machine which owns the player instance.
            Camera camera = GetComponentInChildren<Camera>(true);
            if(camera) { camera.gameObject.SetActive(IsLocalPlayer); }
        }

        /**
         * Called for each frame of the game's runtime on all clients (including the host).
         */
        public void Update()
        {
            //If the machine calling update is not the owner of the player return from the update method. We don't want to allow clients to move other players.
            if (!IsOwner) return;

            //Localised jump.
            if(IsGrounded && Input.GetKeyDown(m_jumpKey))
            {
                m_rigidbody.AddForce(m_jumpForce * transform.up, ForceMode.Impulse);
            }

            //Movement on the player's localised depth and vertical axis. (Forward, backward, left, and right movement).
            m_rigidbody.MovePosition(transform.position + m_moveSpeed * Time.deltaTime * (VerticalAxis * transform.forward + HorizontalAxis * transform.right));

            //Rotation on the global vertical axis. 
            transform.eulerAngles += m_rotateSensitivity * MouseX * Time.deltaTime * Vector3.up;
        }

        #region DEVELOPMENT_ONLY
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //Draw the raycast which will be used to determine whether the player is grounded.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + -transform.up * m_raycastLength * transform.localScale.magnitude);
        }
#endif
        #endregion
    }
}





/*
 * 
 * 
 [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerController : NetworkBehaviour
    {
        //Attributes:
        [Header("Movement Settings")]
        public float m_moveSpeed = 1.0f;
        public KeyCode m_jumpKey = KeyCode.Space;

        [Header("Rotation Settings")]
        public float m_rotateSensitivity = 1.0f;
        public bool m_isInverted = false;

        [Header("Jump Settings")]
        [SerializeField] private float m_jumpForce = 1.0f;
        [SerializeField] private float m_raycastLength = 1.0f;
        private Rigidbody m_rigidbody = null;
        [SerializeField] private Vector3 m_offsetPosition = Vector3.zero;
        [SerializeField] private Vector3 m_offsetEulerAngles = Vector3.zero;

        //Properties:
        private static float HorizontalAxis => Input.GetAxis("Horizontal");
        private static float VerticalAxis => Input.GetAxis("Vertical");
        private bool IsGrounded => Physics.Raycast(transform.position, -transform.up, m_raycastLength * transform.localScale.magnitude);
        private float MouseX => m_isInverted ? Input.GetAxis("Mouse X") : Input.GetAxis("Mouse X") * -1;

        //Methods:
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if(!IsOwner || !IsClient) { return; }

            //Place the player at a random locaton.
            m_rigidbody = GetComponent<Rigidbody>();
            transform.position = new Vector3(Random.Range(-2, 2), 4.0f, Random.Range(-2, 2));
            m_offsetPosition = Vector3.zero;
            m_offsetEulerAngles = Vector3.zero;
            UpdatePositionServerRpc(transform.position);
        }

        [ServerRpc]
        private void JumpServerRpc()
        {
            m_rigidbody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
        }

        [ClientRpc]
        private void JumpClientRpc()
        {
            if(!IsLocalPlayer)
            m_rigidbody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
        }

        [ServerRpc]
        private void MoveServerRpc(Vector3 p_position)
        {
            m_rigidbody.MovePosition(p_position);
        }

        [ClientRpc]
        private void MoveClientRpc(Vector3 p_position)
        {
            if(!IsLocalPlayer) { return; }

            m_rigidbody.MovePosition(p_position);
        }

        [ServerRpc]
        private void RequestJumpServerRpc()
        {

        }

        private void Update()
        {
            if(IsServer && IsOwnedByServer) { UpdatePositionClientRpc(transform.position); }
            if (!IsOwner) { return; }

            //Movement:
            // m_offsetPosition += (transform.right * HorizontalAxis + transform.forward * VerticalAxis) * m_moveSpeed * Time.deltaTime;
            //RpcMove()

            //Rotate:
            //m_offsetEulerAngles += MouseX * m_rotateSensitivity * Time.deltaTime * Vector3.up;

            //Jump:
            if (IsGrounded && Input.GetKey(KeyCode.Space)) { m_rigidbody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse); }
            UpdatePositionServerRpc(transform.position);
        }

        [ClientRpc]
        private void UpdatePositionClientRpc(Vector3 p_position)
        {
            if (IsOwner) { return; }
            transform.position = p_position;
        }

        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 p_position)
        {
            if (IsOwner) { return; }
            transform.position = p_position;

            //Update all of the clients.
            UpdatePositionClientRpc(p_position);
        }

        private void UpdateHost()
        {
            //Update rotation.
            transform.eulerAngles += m_offsetEulerAngles;

            //Movement.
            m_rigidbody.MovePosition(transform.position + m_offsetPosition);

            //Jump:
            m_rigidbody.AddForce(m_offsetPosition, ForceMode.Impulse);

            //Reset offsets.
            m_offsetPosition = Vector3.zero;
            m_offsetEulerAngles = Vector3.zero;
        }

        #region DEVELOPMENT_ONLY
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + -transform.up * m_raycastLength * transform.localScale.magnitude);
        }
#endif
        #endregion
    }
*/