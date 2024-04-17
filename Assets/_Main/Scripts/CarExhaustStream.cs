using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExhaustStream : MonoBehaviour
{

    [SerializeField] private CarController carController;
    [SerializeField] private float maxStreamLength = 5f;
    [SerializeField] private float fireDuration = 0.2f;

    private new MeshRenderer renderer;
    private float targetStreamLength = 0;
    private float streamLength = 0;
    private float streamLengthChangePerSecond;

    private void Start()
    {
        this.renderer = GetComponent<MeshRenderer>();
        this.renderer.enabled = false;
        this.streamLengthChangePerSecond = this.maxStreamLength / this.fireDuration;
    }

    private void Update()
    {
        this.UpdateTargetStreamLength();
        this.UpdateStreamLength();
    }

    private void UpdateTargetStreamLength()
    {
        if (this.carController.IsNitro)
        {
            this.targetStreamLength = this.maxStreamLength;
        }
        else
        {
            this.targetStreamLength = 0f;
        }
    }

    private void UpdateStreamLength()
    {
        this.streamLength = Mathf.MoveTowards(this.streamLength, this.targetStreamLength, this.streamLengthChangePerSecond * Time.deltaTime);
        this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, streamLength);
        if (this.streamLength > 0.1f)
        {
            this.renderer.enabled = true;
        } else
        {
            this.renderer.enabled = false;
        }
    }

}
