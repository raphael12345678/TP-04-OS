using UnityEngine;
using UnityEngine.InputSystem;

// Exercice 3 : déplacement avec le joystick gauche, rotation par crans avec le joystick droit.
// À placer sur l'objet "XR Origin (XR Rig)".
public class VRJoystickLocomotion : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Glisser ici la Main Camera (enfant de Camera Offset)")]
    public Transform head;

    [Header("Déplacement (joystick gauche)")]
    public float moveSpeed = 2f;

    [Header("Rotation (joystick droit)")]
    [Tooltip("Angle de rotation par cran : 30 ou 45")]
    public float snapAngle = 45f;
    [Tooltip("Inclinaison du joystick nécessaire pour déclencher une rotation")]
    public float turnThreshold = 0.7f;

    private InputAction moveAction;
    private InputAction turnAction;
    private bool canTurn = true;
    private CharacterController characterController;

    void Awake()
    {
        // Joystick gauche (Primary2DAxis = thumbstick sur Meta Quest et dans le simulateur)
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddBinding("<XRController>{LeftHand}/{Primary2DAxis}");

        // Joystick droit
        turnAction = new InputAction("Turn", InputActionType.Value);
        turnAction.AddBinding("<XRController>{RightHand}/{Primary2DAxis}");

        if (head == null && Camera.main != null)
            head = Camera.main.transform;

        // Le XR Origin a un Character Controller : on s'en sert pour bouger (collisions)
        characterController = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        moveAction.Enable();
        turnAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        turnAction.Disable();
    }

    void Update()
    {
        HandleMove();
        HandleSnapTurn();
    }

    void HandleMove()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.01f) return; // zone morte

        // On avance dans la direction où l'on regarde, mais sans monter ni descendre
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;

        Vector3 direction = forward * input.y + right * input.x;
        Vector3 displacement = direction * moveSpeed * Time.deltaTime;

        if (characterController != null && characterController.enabled)
            characterController.Move(displacement);
        else
            transform.position += displacement;
    }

    void HandleSnapTurn()
    {
        float x = turnAction.ReadValue<Vector2>().x;

        if (canTurn && Mathf.Abs(x) > turnThreshold)
        {
            // On tourne autour de la tête pour que le joueur pivote sur place
            transform.RotateAround(head.position, Vector3.up, Mathf.Sign(x) * snapAngle);
            Physics.SyncTransforms(); // informe le Character Controller de la nouvelle position
            canTurn = false; // un seul cran par poussée
        }
        else if (Mathf.Abs(x) < 0.2f)
        {
            canTurn = true; // le joystick est revenu au centre
        }
    }
}