using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/**
 *  A first person controller component which implements a player's jump, movement, and camera rotation logic.
 *  
 *  @owner Thomas Jacobs.
 *  @link https://thomasjacobs.itch.io/unity-first-person-controller
 */
[RequireComponent(typeof(Rigidbody))] public sealed class Controller3D : NetworkBehaviour
{
    //Attributes:
    [Header("Jump Settings:")]
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] private float m_raycastLength = 1.0f;

    [Header("Movement Settings:")]
    [Range(0.01f, 62.0f), SerializeField] private float m_walkSpeed = 30.00f;
    [Range(0.01f, 62.0f), SerializeField] private float m_sprintSpeed = 40.00f;

    [Header("Camera Settings")]
    [Range(0.01f, 10.0f), SerializeField] private float m_mouseSensitivity = 5f;
    [SerializeField] private float m_minimumEulerAngle = 0.0f;
    [SerializeField] private float m_maximumEulerAngle = 1.0f;
    private float m_cameraHorizontalEulerAngles = 0.0f;

    private Rigidbody m_rigidbody = null;
    private Camera m_camera = null;

    //Properties:
    private float Velocity => Input.GetKey(KeyCode.LeftShift) ? m_sprintSpeed : m_walkSpeed;
    private float HorizontalAxis  => Input.GetAxis("Horizontal");
    private float VerticalAxis => Input.GetAxis("Vertical");
    private float MouseX => Input.GetAxis("Mouse X");
    private float MouseY => Input.GetAxis("Mouse Y");
    private bool IsGrounded => Physics.Raycast(transform.position, -transform.up, m_raycastLength);

    //Methods:
    void Awake()
    {
        //Initialise component variables.
        m_camera = gameObject.GetComponentInChildren<Camera>();
        m_rigidbody = GetComponent<Rigidbody>();

        //The camera or rigidbody have been removed from the prefabrication. Ensure the camera is allocated it's own game-object and is parented under the game-object with this script.
        UnityEngine.Assertions.Assert.IsTrue(m_camera || m_rigidbody);

        //Cache and pre-calculate frequently used values.
        m_raycastLength *= transform.localScale.magnitude;
        m_sprintSpeed *= transform.localScale.magnitude;
        m_walkSpeed *= transform.localScale.magnitude;
    }

    private void Update()
    {
        //Only the client which owns the instance should have control over the player controller.
        if(!IsOwner) { return; }

        //Rigidbody movement and rotation.
        m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.Euler(new Vector3(default, MouseX * m_mouseSensitivity, default))); //Rotation
        m_rigidbody.MovePosition(transform.position + Time.deltaTime * this.Velocity * (transform.forward * VerticalAxis + transform.right * HorizontalAxis));

        //Camera rotation.
        m_cameraHorizontalEulerAngles = Mathf.Clamp(m_cameraHorizontalEulerAngles + m_mouseSensitivity * -MouseY, m_minimumEulerAngle, m_maximumEulerAngle);
        m_camera.transform.localRotation = Quaternion.Euler(Vector3.right * m_cameraHorizontalEulerAngles);

        //Jump:
        if (Input.GetKeyDown(m_jumpKey) && IsGrounded) { m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse); }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(m_minimumEulerAngle < m_maximumEulerAngle) { return; }

        //If the minimum euler angle is larger than the maximum euler angle, swap the two values.
        float copy = m_minimumEulerAngle;
        m_minimumEulerAngle = m_maximumEulerAngle;
        m_maximumEulerAngle = copy;
    }

    private const float LINE_LENGTH = 4.0f;

    private void OnDrawGizmos()
    {
        //Draw raycast.
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * (Application.isPlaying ? m_raycastLength : m_raycastLength * transform.localScale.magnitude));

        if(!m_camera)
        {
            m_camera = GetComponentInChildren<Camera>();

            //Any game-object with the 'Controller3D' script must have a child game-object containing a Unity's camera component.
            UnityEngine.Assertions.Assert.IsNotNull<Camera>(m_camera);
        }

        //Draw minimum euler angle.
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(m_camera.transform.position, Quaternion.Euler(m_minimumEulerAngle, m_camera.transform.localEulerAngles.y, m_camera.transform.localEulerAngles.z), Gizmos.matrix.lossyScale);
        Gizmos.DrawLine(m_camera.transform.forward * LINE_LENGTH * transform.localScale.magnitude, Vector3.zero);

        //Draw maximum euler angle.
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(m_camera.transform.position, Quaternion.Euler(m_maximumEulerAngle, m_camera.transform.localEulerAngles.y, m_camera.transform.localEulerAngles.z), Gizmos.matrix.lossyScale);
        Gizmos.DrawLine(m_camera.transform.forward * LINE_LENGTH * transform.localScale.magnitude, Vector3.zero);
    }
#endif
}