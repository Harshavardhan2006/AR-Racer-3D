using UnityEngine;

public static class CarFactory
{
    public static GameObject Build(Transform parent, Material template, Color paint)
    {
        var root = new GameObject("CarModel");
        root.transform.SetParent(parent, false);

        Material body = Make(template, paint, 0.85f, 0.55f);
        Material glass = Make(template, new Color(0.06f, 0.1f, 0.16f), 0.95f, 0.2f);
        Material tire = Make(template, new Color(0.04f, 0.04f, 0.04f), 0.15f, 0f);
        Material rim = Make(template, new Color(0.85f, 0.85f, 0.9f), 0.9f, 0.9f);
        Material head = Make(template, new Color(1f, 0.96f, 0.75f), 0.6f, 0f);
        Material tail = Make(template, new Color(0.9f, 0.05f, 0.05f), 0.6f, 0f);
        Material trim = Make(template, new Color(0.08f, 0.08f, 0.08f), 0.4f, 0.2f);

        Part(root, PrimitiveType.Cube, "Chassis", new Vector3(0f, 0.22f, 0f), new Vector3(1.05f, 0.28f, 2.3f), Vector3.zero, body);
        Part(root, PrimitiveType.Cube, "Hood", new Vector3(0f, 0.4f, 0.75f), new Vector3(0.95f, 0.1f, 0.7f), new Vector3(-6f, 0f, 0f), body);
        Part(root, PrimitiveType.Cube, "Trunk", new Vector3(0f, 0.4f, -0.8f), new Vector3(0.95f, 0.1f, 0.5f), new Vector3(5f, 0f, 0f), body);
        Part(root, PrimitiveType.Cube, "Cabin", new Vector3(0f, 0.55f, -0.1f), new Vector3(0.85f, 0.3f, 1.05f), Vector3.zero, glass);
        Part(root, PrimitiveType.Cube, "Roof", new Vector3(0f, 0.72f, -0.1f), new Vector3(0.82f, 0.05f, 0.85f), Vector3.zero, body);
        Part(root, PrimitiveType.Cube, "Windshield", new Vector3(0f, 0.58f, 0.42f), new Vector3(0.82f, 0.3f, 0.05f), new Vector3(-35f, 0f, 0f), glass);
        Part(root, PrimitiveType.Cube, "RearGlass", new Vector3(0f, 0.58f, -0.62f), new Vector3(0.82f, 0.3f, 0.05f), new Vector3(35f, 0f, 0f), glass);
        Part(root, PrimitiveType.Cube, "Bumper_F", new Vector3(0f, 0.15f, 1.17f), new Vector3(1.08f, 0.12f, 0.1f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "Bumper_R", new Vector3(0f, 0.15f, -1.17f), new Vector3(1.08f, 0.12f, 0.1f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "Spoiler", new Vector3(0f, 0.65f, -1.05f), new Vector3(0.95f, 0.04f, 0.25f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "SpoilerL", new Vector3(-0.35f, 0.54f, -1.05f), new Vector3(0.04f, 0.2f, 0.12f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "SpoilerR", new Vector3(0.35f, 0.54f, -1.05f), new Vector3(0.04f, 0.2f, 0.12f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "StripeL", new Vector3(-0.12f, 0.455f, 0.1f), new Vector3(0.1f, 0.01f, 2.2f), Vector3.zero, trim);
        Part(root, PrimitiveType.Cube, "StripeR", new Vector3(0.12f, 0.455f, 0.1f), new Vector3(0.1f, 0.01f, 2.2f), Vector3.zero, trim);

        for (int side = -1; side <= 1; side += 2)
        {
            Part(root, PrimitiveType.Cube, "HeadLight", new Vector3(0.36f * side, 0.3f, 1.16f), new Vector3(0.22f, 0.1f, 0.05f), Vector3.zero, head);
            Part(root, PrimitiveType.Cube, "TailLight", new Vector3(0.36f * side, 0.3f, -1.16f), new Vector3(0.22f, 0.1f, 0.05f), Vector3.zero, tail);
            Part(root, PrimitiveType.Cube, "Mirror", new Vector3(0.5f * side, 0.55f, 0.3f), new Vector3(0.1f, 0.07f, 0.07f), Vector3.zero, body);
        }

        float[] xs = { -0.56f, 0.56f };
        float[] zs = { -0.75f, 0.75f };
        foreach (float x in xs)
        {
            foreach (float z in zs)
            {
                var wheel = new GameObject("Wheel");
                wheel.transform.SetParent(root.transform, false);
                wheel.transform.localPosition = new Vector3(x, 0.2f, z);
                Part(wheel, PrimitiveType.Cylinder, "Tire", Vector3.zero, new Vector3(0.42f, 0.11f, 0.42f), new Vector3(0f, 0f, 90f), tire);
                Part(wheel, PrimitiveType.Cylinder, "Rim", new Vector3(Mathf.Sign(x) * 0.115f, 0f, 0f), new Vector3(0.26f, 0.012f, 0.26f), new Vector3(0f, 0f, 90f), rim);
            }
        }

        return root;
    }

    static Material Make(Material template, Color color, float smoothness, float metallic)
    {
        var m = new Material(template);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        return m;
    }

    static void Part(GameObject parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Vector3 euler, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
}
