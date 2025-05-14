using System;
using UnityEngine;

public class ClawSuccessDetector : MonoBehaviour
{
    public LocateAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            Debug.Log("Success");
            agent.GoalReached();
        }
    }

}