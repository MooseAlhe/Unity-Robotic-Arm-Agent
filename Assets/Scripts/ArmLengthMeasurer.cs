using UnityEngine;

public class ArmLengthMeasurer : MonoBehaviour
{
    [SerializeField] private Transform baseJoint;
    [SerializeField] private Transform tip;

    void Update()
    {
        if (baseJoint != null && tip != null)
        {
            float distance = Vector3.Distance(baseJoint.position, tip.position);
            Debug.Log($"Distance from base to tip: {distance:F3} units");
        }
    }
} 