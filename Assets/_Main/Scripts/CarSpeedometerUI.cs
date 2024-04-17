using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarSpeedometerUI : MonoBehaviour
{

    [SerializeField] private TMP_Text speedText;
    [SerializeField] private RectTransform speedIndicatorMask;
    [SerializeField] private RectTransform speedIndicator;
    [SerializeField] private CarController car;
    [SerializeField] private float maxSpeed = 400f;
    [SerializeField] private float maskMaxOffset = 250f;

    private void Start()
    {
        InvokeRepeating(nameof(UpdateSpeed), 0, 0.1f);
    }

    private void UpdateSpeed()
    {
        float speed = this.car.SpeedKmh;
        float speedFactor = Mathf.InverseLerp(this.maxSpeed, 0f, speed);
        float offset = this.maskMaxOffset * speedFactor;
        this.speedText.text = ((int)speed).ToString();
        this.speedIndicatorMask.anchoredPosition = new Vector2(-offset, 0);
        this.speedIndicator.anchoredPosition = new Vector2(offset, 0);
    }

}
