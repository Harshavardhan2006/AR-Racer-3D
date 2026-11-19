using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    [Header("Track dimensions in metres")]
    [SerializeField] private float outerX = 0.85f;
    [SerializeField] private float outerZ = 1.30f;
    [SerializeField] private float innerX = 0.48f;
    [SerializeField] private float innerZ = 0.93f;
    [SerializeField] private float roadHeight = 0.04f;
    [SerializeField] private int segments = 80;

    [Header("Optional car model")]
    [SerializeField] private GameObject carModelPrefab;
    [SerializeField] private float carModelScale = 0.2f;
    [SerializeField] private float carModelYaw = 0f;
    [SerializeField] private float carModelHeight = 0.026f;
    [SerializeField] private Material carModelMaterial;
    [SerializeField] private Vector3 carColliderSize = new Vector3(0.18f, 0.18f, 0.3f);

    [Header("Optional materials")]
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Material barrierMaterial;
    [SerializeField] private Material carBodyMaterial;
    [SerializeField] private Material coinMaterial;

    private const float BarrierHeight = 0.16f;
    private const float BarrierThickness = 0.055f;

    private Material fallbackRoadMaterial;
    private Material fallbackBarrierMaterial;
    private Material fallbackCarMaterial;
    private Material fallbackCoinMaterial;
    private Material fallbackCabinMaterial;
    private Material fallbackWheelMaterial;

    private void Start()
    {
        segments = Mathf.Max(segments, 24);
        BuildRoad();
        BuildBarriers();
        BuildCheckpoints();
        SpawnCoins();
        SpawnCar();
    }

    private void BuildRoad()
    {
        GameObject road = new GameObject("GeneratedRoad");
        road.transform.SetParent(transform, false);

        MeshFilter meshFilter = road.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = road.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = road.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh
        {
            name = "OvalTrackMesh"
        };

        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            vertices[i * 2] =
                new Vector3(x * outerX, roadHeight, z * outerZ);

            vertices[i * 2 + 1] =
                new Vector3(x * innerX, roadHeight, z * innerZ);

            uv[i * 2] = new Vector2(0f, (float)i / segments);
            uv[i * 2 + 1] = new Vector2(1f, (float)i / segments);

            if (i < segments)
            {
                int t = i * 6;
                int v = i * 2;

                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;

                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.sharedMaterial = roadMaterial != null
            ? roadMaterial
            : GetFallbackMaterial(ref fallbackRoadMaterial,
                new Color(0.16f, 0.17f, 0.19f));

        meshRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = true;
    }

    private void BuildBarriers()
    {
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * Mathf.PI * 2f / segments;
            float a2 = (i + 1) * Mathf.PI * 2f / segments;

            CreateBarrierSegment(
                outerX * Mathf.Cos(a1),
                outerZ * Mathf.Sin(a1),
                outerX * Mathf.Cos(a2),
                outerZ * Mathf.Sin(a2),
                "OuterBarrier");

            CreateBarrierSegment(
                innerX * Mathf.Cos(a1),
                innerZ * Mathf.Sin(a1),
                innerX * Mathf.Cos(a2),
                innerZ * Mathf.Sin(a2),
                "InnerBarrier");
        }
    }

    private void CreateBarrierSegment(
        float x1, float z1, float x2, float z2, string objectName)
    {
        Vector3 start = new Vector3(x1, roadHeight + BarrierHeight * 0.5f, z1);
        Vector3 end = new Vector3(x2, roadHeight + BarrierHeight * 0.5f, z2);
        Vector3 direction = end - start;

        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.name = objectName;
        barrier.transform.SetParent(transform, false);
        barrier.transform.localPosition = (start + end) * 0.5f;
        barrier.transform.localRotation =
            Quaternion.LookRotation(direction.normalized);
        barrier.transform.localScale =
            new Vector3(BarrierThickness, BarrierHeight, direction.magnitude + 0.025f);

        Renderer renderer = barrier.GetComponent<Renderer>();
        renderer.sharedMaterial = barrierMaterial != null
            ? barrierMaterial
            : GetFallbackMaterial(ref fallbackBarrierMaterial,
                new Color(0.85f, 0.12f, 0.10f));
    }

    private void BuildCheckpoints()
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 0.5f;
            Vector3 radial = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            Vector3 tangent = new Vector3(
                -Mathf.Sin(angle), 0f, Mathf.Cos(angle));

            float midX = (outerX + innerX) * 0.5f;
            float midZ = (outerZ + innerZ) * 0.5f;

            Vector3 localPosition = new Vector3(
                radial.x * midX,
                roadHeight + 0.20f,
                radial.z * midZ);

            GameObject gate = new GameObject("Checkpoint_" + i);
            gate.transform.SetParent(transform, false);
            gate.transform.localPosition = localPosition;
            gate.transform.localRotation =
                Quaternion.LookRotation(tangent);

            BoxCollider trigger = gate.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(
                outerX - innerX + 0.30f,
                0.50f,
                0.18f);

            RaceCheckpoint checkpoint =
                gate.AddComponent<RaceCheckpoint>();
            checkpoint.SetIndex(i);
        }
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = (i + 0.5f) * Mathf.PI * 2f / 8f;
            float midX = (outerX + innerX) * 0.5f;
            float midZ = (outerZ + innerZ) * 0.5f;

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * midX,
                roadHeight + 0.22f,
                Mathf.Sin(angle) * midZ);

            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin_" + i;
            coin.transform.SetParent(transform, false);
            coin.transform.localPosition = position;
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            coin.transform.localScale = new Vector3(0.13f, 0.025f, 0.13f);

            Collider coinCollider = coin.GetComponent<Collider>();
            coinCollider.isTrigger = true;

            Renderer renderer = coin.GetComponent<Renderer>();
            renderer.sharedMaterial = coinMaterial != null
                ? coinMaterial
                : GetFallbackMaterial(ref fallbackCoinMaterial,
                    new Color(1f, 0.72f, 0.05f));

            coin.AddComponent<CoinController>();
        }
    }

    private void SpawnCar()
    {
        float startAngle = -0.16f;
        float midX = (outerX + innerX) * 0.5f;
        float midZ = (outerZ + innerZ) * 0.5f;

        Vector3 localPosition = new Vector3(
            Mathf.Cos(startAngle) * midX,
            roadHeight - 0.025f,
            Mathf.Sin(startAngle) * midZ);

        Vector3 forward = new Vector3(
            -Mathf.Sin(startAngle), 0f, Mathf.Cos(startAngle));

        GameObject car = new GameObject("PlayerCar");
        car.transform.SetParent(transform, false);
        car.transform.localPosition = localPosition;
        car.transform.localRotation = Quaternion.LookRotation(forward);

        Rigidbody body = car.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        BoxCollider collider = car.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.025f + carColliderSize.y * 0.5f, 0f);
        collider.size = carColliderSize;

        CreateCarVisuals(car.transform);

        CarController controller = car.AddComponent<CarController>();
        controller.Initialize(RaceManager.Instance);

        if (RaceManager.Instance != null)
            RaceManager.Instance.BeginRace(controller);

        HoldButton[] buttons =
            FindObjectsByType<HoldButton>(FindObjectsSortMode.None);

        foreach (HoldButton button in buttons)
            button.SetCar(controller);
    }

    private void CreateCarVisuals(Transform parent)
    {
        if (carModelPrefab != null)
        {
            GameObject imported = Instantiate(carModelPrefab, parent);
            imported.transform.localPosition = new Vector3(0f, carModelHeight, 0f);
            imported.transform.localRotation = Quaternion.Euler(0f, carModelYaw, 0f);
            imported.transform.localScale = Vector3.one * carModelScale;

            foreach (Collider modelCollider in imported.GetComponentsInChildren<Collider>())
                Destroy(modelCollider);

            if (carModelMaterial != null)
            {
                foreach (Renderer modelRenderer in imported.GetComponentsInChildren<Renderer>())
                {
                    Material[] swapped = new Material[modelRenderer.sharedMaterials.Length];

                    for (int i = 0; i < swapped.Length; i++)
                        swapped[i] = carModelMaterial;

                    modelRenderer.sharedMaterials = swapped;
                }
            }

            return;
        }

        Material template = carBodyMaterial != null
            ? carBodyMaterial
            : GetFallbackMaterial(ref fallbackCarMaterial,
                new Color(0.08f, 0.35f, 0.95f));

        if (template == null)
            return;

        GameObject model = CarFactory.Build(
            parent, template, new Color(0.85f, 0.08f, 0.08f));

        model.transform.localPosition = new Vector3(0f, 0.026f, 0f);
        model.transform.localScale = Vector3.one * carModelScale;
    }

    private void CreateWheel(
        Transform parent, float x, float y, float z)
    {
        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "Wheel";
        wheel.transform.SetParent(parent, false);
        wheel.transform.localPosition = new Vector3(x, y, z);
        wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheel.transform.localScale = new Vector3(0.075f, 0.035f, 0.075f);
        Destroy(wheel.GetComponent<Collider>());
        wheel.GetComponent<Renderer>().sharedMaterial =
            GetFallbackMaterial(ref fallbackWheelMaterial,
                new Color(0.035f, 0.035f, 0.04f));
    }

    private Material GetFallbackMaterial(ref Material cachedMaterial, Color color)
    {
        if (cachedMaterial != null)
            return cachedMaterial;

        cachedMaterial = CreateRuntimeMaterial(color);
        return cachedMaterial;
    }

    private Material CreateRuntimeMaterial(Color color)
    {
        string[] candidates =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Sprites/Default",
            "Standard"
        };

        Shader shader = null;

        foreach (string shaderName in candidates)
        {
            shader = Shader.Find(shaderName);

            if (shader != null)
                break;
        }

        if (shader == null)
        {
            Debug.LogError("TrackBuilder could not find any shader. " +
                           "Assign material assets in the Inspector.");
            return null;
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
