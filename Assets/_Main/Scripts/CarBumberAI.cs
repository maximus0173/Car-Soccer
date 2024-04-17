using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBumberAI : MonoBehaviour
{

    [SerializeField] private CarAI carAI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            this.carAI.HitBall();
        }
    }

}
