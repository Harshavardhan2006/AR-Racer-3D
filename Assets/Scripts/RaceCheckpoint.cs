using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{
    [SerializeField] private int checkpointIndex;

    public void SetIndex(int index)
    {
        checkpointIndex = index;
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car =
            other.GetComponentInParent<CarController>();

        if (car != null && RaceManager.Instance != null)
            RaceManager.Instance.PassCheckpoint(checkpointIndex);
    }
}
