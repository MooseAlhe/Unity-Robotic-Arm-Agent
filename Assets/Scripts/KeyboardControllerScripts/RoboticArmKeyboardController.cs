using UnityEngine;
using UnityEngine.UI;

public class RoboticArmKeyboardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Transform baseJoint;
    [SerializeField] Transform shoulderJoint;    
    [SerializeField] Transform elbowJoint;       
    [SerializeField] Transform wristJoint;       
    [SerializeField] Transform clawRotation;     
    [SerializeField] Text[] RotationText;
    [SerializeField] private ClawAnimationController clawController;
    [Header("Settings")]
    [SerializeField] float rotationSpeed = 90f;

    // Store initial rotations
    private Vector3[] initialRotations = new Vector3[5];
    private float[] currentAngles = new float[5];

    // Define rotation limits for each joint
    private readonly Vector2[] rotationLimits = new Vector2[]
    {
        new Vector2(float.MinValue, float.MaxValue),  // Base joint - infinite rotation
        new Vector2(-90f, 90f),    // Shoulder joint limits
        new Vector2(-90f, 90f),    // Elbow joint limits
        new Vector2(-90f, 90f),    // Wrist joint limits
        new Vector2(float.MinValue, float.MaxValue)   // Claw rotation - infinite rotation
    };

    void Start()
    {

        // Store initial rotations
        if (baseJoint != null) initialRotations[0] = baseJoint.localEulerAngles;
        if (shoulderJoint != null) initialRotations[1] = shoulderJoint.localEulerAngles;
        if (elbowJoint != null) initialRotations[2] = elbowJoint.localEulerAngles;
        if (wristJoint != null) initialRotations[3] = wristJoint.localEulerAngles;
        if (clawRotation != null) initialRotations[4] = clawRotation.localEulerAngles;

        Debug.Log("Joint References - Base: " + (baseJoint != null) + 
                  ", Shoulder: " + (shoulderJoint != null) + 
                  ", Elbow: " + (elbowJoint != null) + 
                  ", Wrist: " + (wristJoint != null) +
                  ", Claw Rotation: " + (clawRotation != null));
    }

    void Update()
    {
        // Base rotation (Y-axis) - Q/E keys
        if (Input.GetKey(KeyCode.Q))
            RotateJoint(baseJoint, Vector3.up, 0);
        if (Input.GetKey(KeyCode.W))
            RotateJoint(baseJoint, -Vector3.up, 0);

        // Shoulder joint (Z-axis) - W/S keys
        if (Input.GetKey(KeyCode.A))
            RotateJoint(shoulderJoint, Vector3.forward, 1);
        if (Input.GetKey(KeyCode.S))
            RotateJoint(shoulderJoint, -Vector3.forward, 1);

        // Elbow joint (Z-axis) - A/D keys
        if (Input.GetKey(KeyCode.Z))
            RotateJoint(elbowJoint, Vector3.forward, 2);
        if (Input.GetKey(KeyCode.X))
            RotateJoint(elbowJoint, -Vector3.forward, 2);

        // Wrist joint (Z-axis) - R/F keys
        if (Input.GetKey(KeyCode.E))
            RotateJoint(wristJoint, Vector3.forward, 3);
        if (Input.GetKey(KeyCode.R))
            RotateJoint(wristJoint, -Vector3.forward, 3);

        // Claw rotation (Y-axis) - Y/H keys
        if (Input.GetKey(KeyCode.D))
            RotateJoint(clawRotation, Vector3.up, 4);
        if (Input.GetKey(KeyCode.F))
            RotateJoint(clawRotation, -Vector3.up, 4);

        UpdateRotationDisplays();
    }

    void RotateJoint(Transform joint, Vector3 axis, int jointIndex)
    {
        if (joint == null) return;

        float rotationAmount = rotationSpeed * Time.deltaTime;
        float direction = Mathf.Sign(axis.x + axis.y + axis.z);
        
        // Calculate the new angle
        float newAngle = currentAngles[jointIndex] + (rotationAmount * direction);

        // For base and claw rotation (indices 0 and 4), allow infinite rotation
        // For other joints, check limits
        bool withinLimits = (jointIndex == 0 || jointIndex == 4) ? true : (newAngle >= rotationLimits[jointIndex].x && newAngle <= rotationLimits[jointIndex].y);

        if (withinLimits)
        {
            // Update the current angle and apply rotation
            currentAngles[jointIndex] = newAngle;
            
            // Calculate the actual rotation relative to the initial rotation
            Vector3 targetRotation = initialRotations[jointIndex];
            if (joint == baseJoint || joint == clawRotation)
                targetRotation.y = initialRotations[jointIndex].y + currentAngles[jointIndex];
            else
                targetRotation.z = initialRotations[jointIndex].z + currentAngles[jointIndex];
            
            joint.localEulerAngles = targetRotation;
        }
    }

    void UpdateRotationDisplays()
    {
        if (RotationText == null) return;
        
        for (int i = 0; i < Mathf.Min(RotationText.Length, currentAngles.Length); i++)
        {
            if (RotationText[i] != null)
                RotationText[i].text = currentAngles[i].ToString("F1");
        }
    }

    void OnGUI()
    {
        // Display controls on screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Robotic Arm Controls:", GUI.skin.box);
        GUILayout.Label("Base Rotation: Q/E");
        GUILayout.Label("Shoulder Joint: W/S");
        GUILayout.Label("Elbow Joint: A/D");
        GUILayout.Label("Wrist Joint: R/F");
        GUILayout.Label("Claw Rotation: Y/H");
        GUILayout.EndArea();
    }
}