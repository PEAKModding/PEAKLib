using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;

namespace PEAKLib.Items;

public class TestBall : ItemComponent
{
    public class Data
    {
        public class Color
        {
            public float r = 0;
            public float g = 0;
            public float b = 0;
        }
        public Color color = new Color();
        public int timesUsed = -1;
    }

    int DataID =>  TestsPlugin.Definition.Name.GetHashCode();
    byte[] DataDefault => Serialize(new Data());

    public void Start()
    {
        Data data = GetData();
        if (data.timesUsed < 0)
        {
            Recolor(Color.red);
            OnInstanceDataSet();
        }
    }

    public void Update()
    {

    }

    public void RandomRecolor()
    {
        // random hue, max saturation and value
        Recolor(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f));
    }

    private void Recolor(Color color)
    {
        Data data = GetData();
        this.SetModItemData(DataID, Serialize(new Data()
        {
            color = new Data.Color()
            {
                r = color.r,
                g = color.g,
                b = color.b,
            },
            timesUsed = data.timesUsed + 1,
        }));
        TestsPlugin.Log.LogInfo($"TestBall has been used {data.timesUsed + 1} times.");
    }

    private Data GetData()
    {
        byte[] rawData = this.GetModItemData(DataID, DataDefault);
        Data? data = Deserialize(rawData);
        if (data == null)
        {
            throw new NullReferenceException("Failed to read Data.\n" +
                $"Bytes: {ItemData.BytesToHex(rawData)}\n" +
                $"String: {Encoding.UTF8.GetString(rawData)}.");
        }
        return data;
    }

    byte[] Serialize(Data data)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
    }

    Data? Deserialize(byte[] data)
    {
        return JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(data));
    }

    // This can be called before Start(), so be careful.
    // Use Awake() if you need to initialize things before this is called.
    public override void OnInstanceDataSet()
    {
        Data data = GetData();
        Color color = new Color(data.color.r, data.color.g, data.color.b);
        var meshRenderer = transform.Find("Sphere")?.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.SetColor("_BaseColor", color);
        }
    }
}