using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarController : MonoBehaviour
{

    [SerializeField] private Rigidbody motorSphereRb;
    [SerializeField] private Transform carCollider;
    [SerializeField] private CinemachineVirtualCamera carCamera;
    [SerializeField] private float normalMaxSpeed = 200f;               // in km/h
    [SerializeField] private float nitroMaxSpeed = 350f;                // in km/h
    [SerializeField] private float minSpeedToUseNitro = 50f;            // in km/h
    [SerializeField] private float normalAccelerationPerSecond = 150f;  // in km/h per second
    [SerializeField] private float nitroAccelerationPerSecond = 300f;   // in km/h per second
    [SerializeField] private float brakingPerSecond = 300f;             // in km/h per second
    [SerializeField] private float deaccelerationPerSecond = 20f;       // in km/h per second
    [SerializeField] private float turningAnglePerSecond = 100f;        // in degrees per second
    [SerializeField] private float pullDownForce = 5000f;
    [SerializeField] private float rotateTowardGroundPerSecond = 50f;   // in degrees per second

    private float motorSphereRadius;
    private float minChangeDirectionSpeed = 10f;
    private float groundTestDistance = 0.5f;
    private float turnSpeedFactor = 0.05f;
    private float forwardInput, turnInput, nitroInput;
    private float forwardSpeed, lastForwardSpeed, forwardAcceleration = 0f;    // m/s
    private float targetCarSpeed = 0f;
    private bool isGrounded = false;
    private Vector3 groundNormal;
    private bool isBraking = false;
    private bool isNitro = false;
    private bool canReadInputs = true;

    public float ForwardInput { get => this.forwardInput; }

    public float TurnInput { get => this.turnInput; }

    /// <summary>
    /// Signed speed in m/s
    /// </summary>
    public float ForwardSpeed { get => this.forwardSpeed; }

    /// <summary>
    /// Signed speed in km/h
    /// </summary>
    public float SpeedKmh { get => this.forwardSpeed * 3.6f; }

    /// <summary>
    /// Signed speed in mph
    /// </summary>
    public float SpeedMph { get => this.forwardSpeed * 2.236f; }

    public float ForwardAcceleration { get => this.forwardAcceleration; }

    public bool IsGrounded { get => this.isGrounded; }

    public bool IsBraking { get => this.isBraking; }

    public bool IsNitro { get => this.isNitro; }

    private void Start()
    {
        this.motorSphereRb.transform.parent = null;
        this.motorSphereRb.centerOfMass = Vector3.zero;
        this.motorSphereRadius = this.motorSphereRb.GetComponent<SphereCollider>().radius * this.motorSphereRb.transform.localScale.y;
        if (this.carCamera != null)
        {
            this.carCamera.transform.parent = null;
        }
    }

    private void Update()
    {
        this.UpdateCarPosition();
        this.UpdateTurning();
        this.UpdateTargetCarSpeed();
    }

    private void FixedUpdate()
    {
        this.FixedUpdateCalculateSpeed();
        this.FixedUpdateGrounded();
        this.FixedUpdateCarDirection();
        this.FixedUpdateFreeFall();
        this.motorSphereRb.AddForce(transform.forward * this.targetCarSpeed, ForceMode.Acceleration);
    }

    public void SetInputs(float forwardInput, float turnInput, float nitroInput)
    {
        if (!this.canReadInputs)
        {
            return;
        }
        forwardInput = Mathf.Clamp(forwardInput, -1f, 1f);
        turnInput = Mathf.Clamp(turnInput, -1f, 1f);
        nitroInput = Mathf.Clamp(nitroInput, -1f, 1f);
        this.forwardInput = forwardInput;
        this.turnInput = turnInput;
        this.nitroInput = nitroInput;
    }

    private void UpdateCarPosition()
    {
        transform.position = this.motorSphereRb.transform.position;
        this.carCollider.transform.rotation = transform.rotation;
    }

    private void UpdateTurning()
    {
        if (!this.isGrounded)
            return;
        float turnRatio = Mathf.Clamp(this.forwardSpeed * this.turnSpeedFactor, -1f, 1f);
        float angle = this.turnInput * turnRatio * this.turningAnglePerSecond;
        transform.Rotate(Vector3.up, angle * Time.deltaTime);
    }

    private void UpdateTargetCarSpeed()
    {
        float newTargetCarSpeed = 0f;
        float newAccelerationPerSecond = this.normalAccelerationPerSecond;
        this.isBraking = false;
        this.isNitro = false;
        if (this.isGrounded)
        {
            if (this.forwardInput > 0.1f && this.forwardSpeed > -this.minChangeDirectionSpeed)
            {
                // acceleration forward
                newTargetCarSpeed = this.forwardInput * this.normalMaxSpeed;
                // adjust target speed after nitro
                if (newTargetCarSpeed < this.targetCarSpeed)
                {
                    newTargetCarSpeed = this.targetCarSpeed;
                }
                // nitro
                if (this.nitroInput > 0f && this.targetCarSpeed > this.minSpeedToUseNitro)
                {
                    newTargetCarSpeed = this.nitroMaxSpeed;
                    newAccelerationPerSecond = this.nitroAccelerationPerSecond;
                    this.isNitro = true;
                }
            }
            else if (this.forwardInput < -0.1f && this.forwardSpeed < this.minChangeDirectionSpeed)
            {
                // acceleration backward
                newTargetCarSpeed = this.forwardInput * this.normalMaxSpeed;
            }
            else if ((this.forwardInput < -0.1f && this.forwardSpeed > 0f) || (this.forwardInput > 0.1f && this.forwardSpeed < 0f))
            {
                // braking
                newTargetCarSpeed = 0f;
                newAccelerationPerSecond = this.brakingPerSecond;
                this.isBraking = true;
            }
            else
            {
                // free deacceleration
                newTargetCarSpeed = 0f;
                newAccelerationPerSecond = this.deaccelerationPerSecond;
            }
        }

        // reduce target speed to current car speed (ie. after hit)
        if (newTargetCarSpeed == 0f && this.targetCarSpeed > Mathf.Abs(this.SpeedKmh))
        {
            this.targetCarSpeed = this.SpeedKmh;
        }

        this.targetCarSpeed = Mathf.MoveTowards(this.targetCarSpeed, newTargetCarSpeed, newAccelerationPerSecond * Time.deltaTime);
    }

    private void FixedUpdateCalculateSpeed()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(this.motorSphereRb.velocity);
        this.forwardSpeed = localVelocity.z;
        this.forwardAcceleration = (forwardSpeed - this.lastForwardSpeed) / Time.fixedDeltaTime;
        this.lastForwardSpeed = forwardSpeed;
    }

    private void FixedUpdateGrounded()
    {
        float distance = this.motorSphereRadius + this.groundTestDistance;
        if (Physics.Raycast(this.motorSphereRb.position, Vector3.down, out RaycastHit hit, distance))
        {
            this.isGrounded = true;
            this.groundNormal = hit.normal;
        }
        else
        {
            this.isGrounded = false;
        }
    }

    private void FixedUpdateCarDirection()
    {
        if (this.isGrounded)
        {
            // rotate car according to ground
            Quaternion newRotation = Quaternion.FromToRotation(transform.up, this.groundNormal) * transform.rotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, this.rotateTowardGroundPerSecond * Time.deltaTime);
        }
    }

    private void FixedUpdateFreeFall() 
    {
        if (!this.isGrounded)
        {
            // pull down if in the air
            this.motorSphereRb.AddForce(Vector3.down * this.pullDownForce, ForceMode.Force);
        }
    }

    public void MoveToPosition(Vector3 pos, Quaternion rot)
    {
        this.motorSphereRb.position = pos;
        this.motorSphereRb.rotation = rot;
        this.motorSphereRb.velocity = Vector3.zero;
        this.motorSphereRb.angularVelocity = Vector3.zero;
        this.transform.rotation = rot;
        this.forwardSpeed = 0f;
        this.lastForwardSpeed = 0f;
        this.forwardAcceleration = 0f;
        this.targetCarSpeed = 0f;
    }

    public void Freeze()
    {
        this.canReadInputs = false;
        this.forwardInput = 0f;
        this.turnInput = 0f;
        this.nitroInput = 0f;
        this.isBraking = true;
    }

    public void UnFreeze()
    {
        this.canReadInputs = true;
    }

}
