using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using AES.Tools;


public sealed class IapTestPanel : MonoBehaviour
{
    [SerializeField] private string generatedFolder = "IAP/Generated";
    private List<string> _keys = new();
    private Vector2 _scroll;

    private void OnEnable() => Reload();

    [ContextMenu("Reload")]
    private void Reload()
    {
        _keys.Clear();

        foreach (var ta in Resources.LoadAll<TextAsset>(generatedFolder))
        {
            if (ta == null || string.IsNullOrWhiteSpace(ta.text)) continue;

            try
            {
                var jo = JObject.Parse(ta.text);
                if (!string.Equals(jo["mode"]?.ToString(), "IapProductJson", StringComparison.Ordinal)) continue;

                var rows = jo["rows"] as JArray;
                if (rows == null) continue;

                foreach (var r in rows)
                {
                    var k = r?["ProductKey"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(k)) _keys.Add(k.Trim());
                }
            }
            catch { }
        }

        _keys.Sort();
        _keys = _keys.Distinct().ToList();
        Debug.Log($"[IAP TEST] Loaded {_keys.Count} ProductKey(s).");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 420, Screen.height - 20), GUI.skin.box);

        GUILayout.Label("IAP TEST");
        GUILayout.Label($"IAP.IsBound={IAP.IsBound}, IsReady={IAP.IsReady}");

        if (GUILayout.Button("Reload Keys")) Reload();

        _scroll = GUILayout.BeginScrollView(_scroll);
        foreach (var k in _keys)
        {
            if (GUILayout.Button($"Buy: {k}", GUILayout.Height(26)))
                _ = SafeRun(() => IAP.PurchaseByProductKeyAsync(k));
        }
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private async static UniTaskVoid SafeRun(Func<UniTask> f)
    {
        try { await f(); }
        catch (Exception e) { Debug.LogWarning($"[IAP TEST] {e.Message}"); }
    }
}
