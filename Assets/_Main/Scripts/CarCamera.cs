using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarCamera : MonoBehaviour
{

    [SerializeField] private CarController carController;
    [SerializeField] private CinemachineVirtualCamera carCamera;

    [SerializeField] private float zForwardDamping = 2f;
    [SerializeField] private float zBackwardDamping = 0.2f;
    [SerializeField] private float zDampingChangeTime = 0.5f;    // in seconds

    private CinemachineTransposer carCameraTransposer;
    private float zDampingChangePerSecond;

    private void Start()
    {
        this.carCameraTransposer = this.carCamera.GetCinemachineComponent<CinemachineTransposer>();
        float zDampingDiff = Mathf.Abs(this.zForwardDamping - this.zBackwardDamping);
        this.zDampingChangePerSecond = zDampingDiff / this.zDampingChangeTime;
    }

    private void Update()
    {
        float newZDamping = this.zForwardDamping;
        if (this.carController.ForwardSpeed < 0)
        {
            newZDamping = this.zBackwardDamping;
        }
        this.carCameraTransposer.m_ZDamping = Mathf.MoveTowards(this.carCameraTransposer.m_ZDamping, newZDamping, this.zDampingChangePerSecond * Time.deltaTime);
    }

}
