using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Arcade driving")]
    [SerializeField] private float acceleration = 2.8f;
    [SerializeField] private float maximumForwardSpeed = 1.7f;
    [SerializeField] private float maximumReverseSpeed = 0.65f;
    [SerializeField] private float steeringDegreesPerSecond = 105f;
    [SerializeField] private float brakeRate = 5f;

    private Rigidbody body;
    private RaceManager raceManager;
    private float throttleInput;
    private float steeringInput;
    private bool braking;

    public float CurrentSpeedKmh { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Initialize(RaceManager manager)
    {
        raceManager = manager;
    }

    private void FixedUpdate()
    {
        if (body == null)
            return;

        if (raceManager != null && raceManager.IsRaceFinished)
        {
            body.linearVelocity = Vector3.zero;
            return;
        }

        float forwardSpeed = Vector3.Dot(
            body.linearVelocity, transform.forward);

        if (braking)
        {
            forwardSpeed = Mathf.MoveTowards(
                forwardSpeed, 0f, brakeRate * Time.fixedDeltaTime);
        }
        else
        {
            float targetSpeed = throttleInput >= 0f
                ? maximumForwardSpeed * throttleInput
                : maximumReverseSpeed * throttleInput;

            forwardSpeed = Mathf.MoveTowards(
                forwardSpeed,
                targetSpeed,
                acceleration * Time.fixedDeltaTime);
        }

        body.linearVelocity = transform.forward * forwardSpeed;

        float steerReference = Mathf.Abs(forwardSpeed) > 0.08f
            ? forwardSpeed
            : throttleInput;

        if (Mathf.Abs(steerReference) > 0.08f)
        {
            float direction = steerReference >= 0f ? 1f : -1f;
            float turn = steeringInput *
                         steeringDegreesPerSecond *
                         direction *
                         Time.fixedDeltaTime;

            body.MoveRotation(
                body.rotation * Quaternion.Euler(0f, turn, 0f));
        }

        CurrentSpeedKmh = Mathf.Abs(forwardSpeed) * 3.6f;

        if (raceManager != null)
            raceManager.UpdateSpeed(CurrentSpeedKmh);
    }

    public void SetAcceleration(float value)
    {
        throttleInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetSteering(float value)
    {
        steeringInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetBrake(bool value)
    {
        braking = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.EndsWith("Barrier"))
            body.linearVelocity *= 0.45f;
    }
}
