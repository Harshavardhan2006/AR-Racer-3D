using UnityEngine;

public class CoinController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 120f;

    private void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CarController>() == null)
            return;

        if (RaceManager.Instance != null)
            RaceManager.Instance.AddCoin();

        Destroy(gameObject);
    }
}
