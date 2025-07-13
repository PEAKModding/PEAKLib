using PEAKLib.Items;
using UnityEngine;

namespace PEAKLib.Tests;

public class TestBall : ModItemComponent
{
    public class Data
    {
        public class CustomColor
        {
            public float r = 0;
            public float g = 0;
            public float b = 0;
        }

        public CustomColor color = new();
        public int timesUsed = -1;
    }

    public void Start()
    {
        Data data = GetData();
        if (data.timesUsed < 0)
        {
            Recolor(Color.red);
            OnInstanceDataSet();
        }
    }

    public void RandomRecolor()
    {
        // random hue, max saturation and value
        Recolor(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f));
    }

    private void Recolor(Color color)
    {
        Data data = GetData();
        SetModItemDataFromJson(
            new Data()
            {
                color = new Data.CustomColor()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                },
                timesUsed = data.timesUsed + 1,
            }
        );

        TestsPlugin.Log.LogInfo($"TestBall has been used {data.timesUsed + 1} times.");
    }

    private Data GetData()
    {
        if (!TryGetModItemDataFromJson<Data>(out var data))
            data = new();

        return data;
    }

    // This can be called before Start(), so be careful.
    // Use Awake() if you need to initialize things before this is called.
    public override void OnInstanceDataSet()
    {
        Data data = GetData();
        Color color = new(data.color.r, data.color.g, data.color.b);
        var meshRenderer = transform.Find("Sphere")?.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.SetColor("_BaseColor", color);
        }
    }
}
