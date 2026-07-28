using System;
using System.Text;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerformanceLogger : MonoBehaviour
{
    // 프레임 타임
    ProfilerRecorder cpuTotal, cpuMain, cpuRender, gpuTime;
    // 드로우콜 (배칭 방식별)
    ProfilerRecorder dcStandard, dcStaticBatched, dcDynamicBatched, dcInstanced;
    // 배치
    ProfilerRecorder batchStatic, batchDynamic, batchInstanced;
    // 지오메트리 / 렌더링
    ProfilerRecorder setPass, tris, verts, shadowCasters, skinnedMeshes;
    // 텍스처 / 메모리
    ProfilerRecorder usedTexBytes, usedTexCount, videoMem;
    ProfilerRecorder gcAlloc, gcUsed, totalMem, texMem;

    readonly StringBuilder sb = new StringBuilder();
    float timer;
    bool saved;
    string csvPath;
    const float Interval = 0.5f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    static ProfilerRecorder R(ProfilerCategory cat, string name)
    {
        var r = ProfilerRecorder.StartNew(cat, name);
        if (!r.Valid)
            Debug.LogWarning($"[PerformanceLogger] 카운터 없음: {name}");
        return r;
    }

    static long V(ProfilerRecorder r) => r.Valid ? r.LastValue : 0;
    static float Ms(ProfilerRecorder r) => V(r) * 1e-6f;   // ns → ms

    void OnEnable()
    {
        QualitySettings.vSyncCount = 0;

        string stamp = DateTime.Now.ToString("MMdd_HHmmss");
        csvPath = Path.Combine(Application.persistentDataPath, $"perf_{stamp}.csv");

        var RC = ProfilerCategory.Render;

        cpuTotal = R(RC, "CPU Total Frame Time");
        cpuMain = R(RC, "CPU Main Thread Frame Time");
        cpuRender = R(RC, "CPU Render Thread Frame Time");
        gpuTime = R(RC, "GPU Frame Time");

        dcStandard = R(RC, "Standard Draw Calls Count");
        dcStaticBatched = R(RC, "Static Batched Draw Calls Count");
        dcDynamicBatched = R(RC, "Dynamic Batched Draw Calls Count");
        dcInstanced = R(RC, "Instanced Batched Draw Calls Count");

        batchStatic = R(RC, "Static Batches Count");
        batchDynamic = R(RC, "Dynamic Batches Count");
        batchInstanced = R(RC, "Instanced Batches Count");

        setPass = R(RC, "SetPass Calls Count");
        tris = R(RC, "Triangles Count");
        verts = R(RC, "Vertices Count");
        shadowCasters = R(RC, "Shadow Casters Count");
        skinnedMeshes = R(RC, "Visible Skinned Meshes Count");

        usedTexBytes = R(RC, "Used Textures Bytes");
        usedTexCount = R(RC, "Used Textures Count");
        videoMem = R(RC, "Video Memory Bytes");

        var MC = ProfilerCategory.Memory;
        gcAlloc = R(MC, "GC Allocated In Frame");
        gcUsed = R(MC, "GC Used Memory");
        totalMem = R(MC, "Total Used Memory");
        texMem = R(MC, "Texture Memory");

        sb.AppendLine("time,scene,cpuTotalMs,cpuMainMs,cpuRenderMs,gpuMs," +
                      "drawCallsTotal,dcStandard,dcStaticBatched,dcDynamicBatched,dcInstanced," +
                      "batchStatic,batchDynamic,batchInstanced," +
                      "setPass,tris,verts,shadowCasters,skinnedMeshes," +
                      "usedTexMB,usedTexCount,videoMemMB," +
                      "gcAllocB,gcUsedMB,totalMemMB,texMemMB");
    }

    void Update()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        timer += Time.unscaledDeltaTime;
        if (timer < Interval) 
            return;

        timer = 0f;

        long dcTotal = V(dcStandard) + V(dcStaticBatched)
                     + V(dcDynamicBatched) + V(dcInstanced);

        sb.AppendLine(
            $"{Time.time:F1},{SceneManager.GetActiveScene().name}," +
            $"{Ms(cpuTotal):F2},{Ms(cpuMain):F2},{Ms(cpuRender):F2},{Ms(gpuTime):F2}," +
            $"{dcTotal},{V(dcStandard)},{V(dcStaticBatched)},{V(dcDynamicBatched)},{V(dcInstanced)}," +
            $"{V(batchStatic)},{V(batchDynamic)},{V(batchInstanced)}," +
            $"{V(setPass)},{V(tris)},{V(verts)},{V(shadowCasters)},{V(skinnedMeshes)}," +
            $"{V(usedTexBytes) / 1048576f:F1},{V(usedTexCount)},{V(videoMem) / 1048576f:F1}," +
            $"{V(gcAlloc)},{V(gcUsed) / 1048576f:F1}," +
            $"{V(totalMem) / 1048576f:F1},{V(texMem) / 1048576f:F1}");
    }

    void OnApplicationQuit() => SaveAll();
    void OnDisable() => SaveAll();

    void SaveAll()
    {
        if (saved) return;
        saved = true;

        var all = new[] {
            cpuTotal, cpuMain, cpuRender, gpuTime,
            dcStandard, dcStaticBatched, dcDynamicBatched, dcInstanced,
            batchStatic, batchDynamic, batchInstanced,
            setPass, tris, verts, shadowCasters, skinnedMeshes,
            usedTexBytes, usedTexCount, videoMem,
            gcAlloc, gcUsed, totalMem, texMem
        };
        foreach (var r in all)
            if (r.Valid) r.Dispose();

        try
        {
            File.WriteAllText(csvPath, sb.ToString());
            Debug.Log($"[PerformanceLogger] saved: {csvPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PerformanceLogger] save failed: {e.Message}");
        }
    }
}