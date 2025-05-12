using UnityEngine;

public class ArmFailureDetector : MonoBehaviour
{
    public RoboticArmAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Fail");
            agent.Fail();
        }
    }
}

