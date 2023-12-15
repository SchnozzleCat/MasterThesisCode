using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public partial class PerformanceStatSystem : SystemBase
{
    private const int MaxNumber = 500;

    private Label _fpsElement;

    private float[] _fpsSamples;

    private NativeReference<int> _pingReference;
    private UIDocument _rootDocument;
    private int _sampleIndex;

    private string[] FPS;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Singleton>();

        _pingReference = new NativeReference<int>(Allocator.Persistent);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;
    }

    protected override void OnStartRunning()
    {
        if (_rootDocument != null)
            return;

        var singleton = SystemAPI.GetSingleton<Singleton>();

        _rootDocument = GameObject.FindWithTag("PerformanceUI").GetComponent<UIDocument>();

        _rootDocument.enabled = true;

        _fpsElement = _rootDocument.rootVisualElement.Q<Label>("FPS");

        _fpsSamples = new float[singleton.FPSSampleCount];

        FPS = new string[MaxNumber + 1];

        for (var i = 0; i <= MaxNumber; i++)
            FPS[i] = i + (i == MaxNumber ? "+" : "");
    }

    protected override void OnStopRunning()
    {
        if (_rootDocument != null)
            _rootDocument.enabled = false;
    }

    protected override void OnDestroy()
    {
        _pingReference.Dispose();
    }

    protected override void OnUpdate()
    {
        UpdateFPS();
    }

    private void UpdateFPS()
    {
        var singleton = SystemAPI.GetSingleton<Singleton>();

        var fps = 1 / SystemAPI.Time.DeltaTime;
        _fpsSamples[_sampleIndex] = fps;
        var avgFps = 0f;
        for (var i = 0; i < singleton.FPSSampleCount; i++)
            avgFps += _fpsSamples[i];
        avgFps /= singleton.FPSSampleCount;
        _sampleIndex = (_sampleIndex + 1) % singleton.FPSSampleCount;
        _fpsElement.text = FPS[(int)math.clamp(avgFps, 0, MaxNumber)];
        _fpsElement.style.color = new StyleColor
        {
            value =
                fps <= singleton.FPSBadBelow
                    ? singleton.BadColor
                    : fps <= singleton.FPSOkayBelow
                        ? singleton.OkayColor
                        : singleton.GoodColor
        };
    }

    [Serializable]
    public struct Singleton : IComponentData
    {
        public Color GoodColor;
        public Color OkayColor;
        public Color BadColor;
        public float FPSOkayBelow;
        public float FPSBadBelow;
        public int FPSSampleCount;
    }
}
