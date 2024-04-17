using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputs : MonoBehaviour
{

    [SerializeField] private CarController carController;

    private GameInputControls inputControls;
    private float verticalInput, horizontalInput;
    private float nitroInput;

    private void Awake()
    {
        this.inputControls = new GameInputControls();
    }

    private void OnEnable()
    {
        this.inputControls.Car.Enable();
    }

    private void OnDisable()
    {
        this.inputControls.Car.Disable();
    }

    private void Update()
    {
        this.UpdateInputs();
        this.carController.SetInputs(this.verticalInput, this.horizontalInput, this.nitroInput);
    }

    private void UpdateInputs()
    {
        Vector2 movement = this.inputControls.Car.Movement.ReadValue<Vector2>();
        this.verticalInput = movement.y;
        this.horizontalInput = movement.x;
        this.nitroInput = this.inputControls.Car.Nitro.ReadValue<float>();
    }

}
