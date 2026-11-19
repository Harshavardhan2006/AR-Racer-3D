using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    public enum ControlAction
    {
        Accelerate,
        Reverse,
        Left,
        Right,
        Brake
    }

    [SerializeField] private CarController car;
    [SerializeField] private ControlAction action;

    public bool IsHeld { get; private set; }

    public void SetCar(CarController target)
    {
        car = target;
    }

    public void Configure(ControlAction value)
    {
        action = value;
    }

    public void SetPressed(bool pressed)
    {
        if (pressed == IsHeld)
            return;

        if (pressed)
            Press();
        else
            ReleaseControl();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Press();
    }

    private void Press()
    {
        IsHeld = true;

        if (car == null)
            car = FindFirstObjectByType<CarController>();

        if (car == null)
            return;

        switch (action)
        {
            case ControlAction.Accelerate:
                car.SetAcceleration(1f);
                break;
            case ControlAction.Reverse:
                car.SetAcceleration(-1f);
                break;
            case ControlAction.Left:
                car.SetSteering(-1f);
                break;
            case ControlAction.Right:
                car.SetSteering(1f);
                break;
            case ControlAction.Brake:
                car.SetBrake(true);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseControl();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseControl();
    }

    private void OnDisable()
    {
        ReleaseControl();
    }

    private void ReleaseControl()
    {
        IsHeld = false;

        if (car == null)
            return;

        switch (action)
        {
            case ControlAction.Accelerate:
            case ControlAction.Reverse:
                car.SetAcceleration(0f);
                break;
            case ControlAction.Left:
            case ControlAction.Right:
                car.SetSteering(0f);
                break;
            case ControlAction.Brake:
                car.SetBrake(false);
                break;
        }
    }
}
