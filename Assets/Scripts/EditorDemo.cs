using UnityEngine;

public class EditorDemo : MonoBehaviour
{
    [SerializeField] private Vector3 trackPosition = new Vector3(0f, 0f, 2.2f);
    [SerializeField] private Vector3 overviewPosition = new Vector3(0f, 2.2f, -0.8f);
    [SerializeField] private bool autoPlaceOnStart;

    private ARTrackPlacement placement;
    private Camera viewCamera;
    private CarController car;
    private bool chase;
    private bool lastForward;
    private bool lastBack;
    private bool lastLeft;
    private bool lastRight;
    private bool lastBrake;
    private GUIStyle boxStyle;

    private void Awake()
    {
        if (!Application.isEditor)
            enabled = false;
    }

    private void Start()
    {
        placement = FindFirstObjectByType<ARTrackPlacement>();
        viewCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();

        if (viewCamera != null)
        {
            foreach (MonoBehaviour behaviour in viewCamera.GetComponents<MonoBehaviour>())
            {
                if (behaviour.GetType().Name.Contains("TrackedPoseDriver"))
                    behaviour.enabled = false;
            }

            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = new Color(0.62f, 0.72f, 0.82f);
        }

        BuildFloor();

        if (autoPlaceOnStart)
            PlaceTrack();
    }

    private void Update()
    {
        if (placement == null)
            return;

        if (!placement.IsTrackPlaced)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetMouseButtonDown(0))
                PlaceTrack();

            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
            chase = true;

        if (Input.GetKeyDown(KeyCode.V))
            chase = false;

        if (Input.GetKeyDown(KeyCode.R))
            ResetDemo();

        if (car == null)
            car = FindFirstObjectByType<CarController>();

        if (car != null)
            Drive();
    }

    private void LateUpdate()
    {
        if (viewCamera == null)
            return;

        Transform view = viewCamera.transform;

        if (chase && car != null)
        {
            Vector3 target = car.transform.position
                             - car.transform.forward * 0.9f
                             + Vector3.up * 0.65f;

            view.position = Vector3.Lerp(view.position, target, Time.deltaTime * 6f);

            Quaternion look = Quaternion.LookRotation(
                car.transform.position + car.transform.forward * 0.3f - view.position);

            view.rotation = Quaternion.Slerp(view.rotation, look, Time.deltaTime * 6f);
            return;
        }

        Vector3 overview = Vector3.Lerp(view.position, overviewPosition, Time.deltaTime * 4f);
        view.position = overview;

        Quaternion overviewLook = Quaternion.LookRotation(trackPosition - view.position);
        view.rotation = Quaternion.Slerp(view.rotation, overviewLook, Time.deltaTime * 4f);
    }

    private void Drive()
    {
        bool forward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool back = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        bool brake = Input.GetKey(KeyCode.Space);

        if (forward != lastForward || back != lastBack)
            car.SetAcceleration(forward == back ? 0f : (forward ? 1f : -1f));

        if (left != lastLeft || right != lastRight)
            car.SetSteering(left == right ? 0f : (left ? -1f : 1f));

        if (brake != lastBrake)
            car.SetBrake(brake);

        lastForward = forward;
        lastBack = back;
        lastLeft = left;
        lastRight = right;
        lastBrake = brake;
    }

    private void PlaceTrack()
    {
        placement.PlaceAt(trackPosition, Quaternion.identity);
    }

    private void ResetDemo()
    {
        if (RaceManager.Instance != null)
            RaceManager.Instance.ResetRace();

        placement.ResetPlacement();
        car = null;
        chase = false;
    }

    private void BuildFloor()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            return;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "DemoFloor";
        floor.transform.position = new Vector3(trackPosition.x, -0.002f, trackPosition.z);
        floor.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Destroy(floor.GetComponent<Collider>());

        Material material = new Material(shader);
        material.color = new Color(0.72f, 0.66f, 0.58f);
        floor.GetComponent<Renderer>().sharedMaterial = material;
    }

    private void OnGUI()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 15;
            boxStyle.padding = new RectOffset(10, 10, 8, 8);
            boxStyle.normal.textColor = Color.white;
        }

        string text = placement != null && placement.IsTrackPlaced
            ? "W A S D or arrows: drive    Space: brake\nC: chase camera    V: overview    R: reset track"
            : "Click or press Enter to place the track\n(this simulates tapping the detected floor)";

        GUI.Box(new Rect(20f, 150f, 480f, 62f), text, boxStyle);
    }
}
