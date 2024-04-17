using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngineSound : MonoBehaviour
{

    [SerializeField] private CarController carController;

    [SerializeField] private AudioSource audioEngine;
    [SerializeField] private float idlePitch = 0.4f;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1f;
    [SerializeField] private float shiftSpeed = 40f;
    [SerializeField] private int maxShift = 5;

    private void Start()
    {
        this.audioEngine.pitch = this.minPitch;
        this.audioEngine.time = Random.Range(0f, 1f);
    }

    private void Update()
    {
        this.UpdateEngineSound();
    }

    private void UpdateEngineSound()
    {
        float absSpeedKmh = Mathf.Abs(this.carController.SpeedKmh);
        int shift = Mathf.CeilToInt(absSpeedKmh / this.shiftSpeed);
        float t = Mathf.InverseLerp(0f, this.shiftSpeed, absSpeedKmh % this.shiftSpeed);
        if (shift > this.maxShift)
        {
            t = 1f;
        }
        float minPitch = this.minPitch;
        if (shift <= 1)
        {
            minPitch = this.idlePitch;
        }
        float pitch = Mathf.Lerp(minPitch, this.maxPitch, t);
        this.audioEngine.pitch = pitch;
    }

}
