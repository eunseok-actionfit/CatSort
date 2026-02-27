using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using AES.Tools;
using Domain;
using UnityEngine;

public sealed class LevelConfigAddressablesRepository : ILevelConfigRepository
{
    private readonly IAssetProvider _assetProvider;

    public LevelConfigAddressablesRepository(IAssetProvider assetProvider)
    {
        _assetProvider = assetProvider;
    }

    // 기본 구현: variant = 0
    public async UniTask<LevelConfig> LoadAsync(
        int levelId,
        int variant = 0,
        CancellationToken ct = default)
    {
        // StageAddressablesImporter에서 만든 address 규칙과 반드시 동일해야 함
        // ex) stage_1125_3.json → address = "stages/1125/3"
        var key = $"stages/{levelId}/{variant}";

        // JSON TextAsset 로드
        var jsonAsset = await _assetProvider.LoadAsync<TextAsset>(key, ct);

        if (jsonAsset == null || string.IsNullOrEmpty(jsonAsset.text))
            throw new System.Exception($"LevelConfigAddressablesRepository: empty json for key={key}");

        // JSON → LevelConfig 역직렬화
        var config = JsonUtility.FromJson<LevelConfig>(jsonAsset.text);
        if (config == null)
            throw new System.Exception($"LevelConfigAddressablesRepository: failed to parse LevelConfig (key={key})");

        return config;
    }
    
    public async UniTask<LevelConfig[]> LoadVariantsAsync(
        int levelId,
        CancellationToken ct = default)
    {
        var label = $"stage-l{levelId}";
        var assets = await _assetProvider.LoadByLabelAsync<TextAsset>(label, ct);

        var list = new List<LevelConfig>();
        foreach (var textAsset in assets)
        {
            var cfg = JsonUtility.FromJson<LevelConfig>(textAsset.text);
            if (cfg != null) list.Add(cfg);
        }

        return list.ToArray();
    }
}