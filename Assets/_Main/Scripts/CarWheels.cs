using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheels : MonoBehaviour
{

    [SerializeField] private CarController carController;

    [SerializeField] private GameObject wheelFL;
    [SerializeField] private GameObject wheelFR;
    [SerializeField] private GameObject wheelRL;
    [SerializeField] private GameObject wheelRR;
    [SerializeField] private float maxWheelSpeed = 500f;
    [SerializeField] private float maxSteerAngle = 30f;

    [SerializeField] private TrailRenderer trailFL;
    [SerializeField] private TrailRenderer trailFR;
    [SerializeField] private TrailRenderer trailRL;
    [SerializeField] private TrailRenderer trailRR;
    [SerializeField] private float trailMinAcceleration = 20f;

    private GameObject[] allWheels;
    private GameObject parentWheelFL;
    private GameObject parentWheelFR;
    private TrailRenderer[] allTrails;

    private void Start()
    {
        this.allWheels = new GameObject[] { this.wheelFL, this.wheelFR, this.wheelRL, this.wheelRR };
        this.CreateSteerWheelsHierarchy();
        this.allTrails = new TrailRenderer[] { this.trailFL, this.trailFR, this.trailRL, this.trailRR };
        InvokeRepeating("UpdateTrails", 1f, 0.1f);
    }

    private void Update()
    {
        this.UpdateWheelsRotation();
        this.UpdateFrontWheelsTurn();
    }

    private void CreateSteerWheelsHierarchy()
    {
        Transform wheelTrL = this.wheelFL.transform;
        Transform wheelTrR = this.wheelFR.transform;
        this.parentWheelFL = Instantiate(new GameObject(), wheelTrL.position, wheelTrL.rotation, wheelTrL.parent);
        this.parentWheelFR = Instantiate(new GameObject(), wheelTrR.position, wheelTrR.rotation, wheelTrR.parent);
        this.parentWheelFL.name = "ParentWheelFL";
        this.parentWheelFR.name = "ParentWheelFR";
        wheelTrL.parent = this.parentWheelFL.transform;
        wheelTrR.parent = this.parentWheelFR.transform;
    }

    private void UpdateWheelsRotation()
    {
        float forwardSpeed = this.carController.ForwardSpeed;
        float speedRatio = Mathf.Clamp(forwardSpeed * 0.1f, -1f, 1f);
        float angle = this.maxWheelSpeed * speedRatio * Time.deltaTime;
        foreach (GameObject wheel in this.allWheels)
        {
            wheel.transform.Rotate(angle, 0f, 0f);
        }
    }

    private void UpdateFrontWheelsTurn()
    {
        float turnInput = this.carController.TurnInput;
        float angle = this.maxSteerAngle * turnInput;
        this.parentWheelFL.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
        this.parentWheelFR.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
    }

    private void UpdateTrails()
    {
        float turnInput = this.carController.TurnInput;
        float forwardCarAcceleration = this.carController.ForwardAcceleration;
        bool isGrounded = this.carController.IsGrounded;
        bool isBraking = this.carController.IsBraking;
        bool isNitro = this.carController.IsNitro;
        bool trailsEmit = false;
        if (isGrounded)
        {
            if (Mathf.Abs(forwardCarAcceleration) > this.trailMinAcceleration && !isNitro)
            {
                trailsEmit = true;
            }
            else if (Mathf.Abs(turnInput) > 0.5f)
            {
                trailsEmit = true;
            }
        }
        if (isBraking && Mathf.Abs(forwardCarAcceleration) < 1f)
        {
            trailsEmit = true;
        }
        foreach (TrailRenderer trail in this.allTrails)
        {
            trail.emitting = trailsEmit;
        }
    }

}
