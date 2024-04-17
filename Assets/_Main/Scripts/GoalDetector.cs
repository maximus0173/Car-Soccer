using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalDetector : MonoBehaviour
{

    [SerializeField] UnityEvent onBallInGoal;

    private void OnTriggerEnter(Collider other)
    {
        if (!this.enabled)
        {
            return;
        }
        if (other.CompareTag("Ball"))
        {
            this.onBallInGoal?.Invoke();
        }
    }

}
