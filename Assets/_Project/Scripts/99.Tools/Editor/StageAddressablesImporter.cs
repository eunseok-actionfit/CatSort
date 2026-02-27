#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public static class StageAddressablesImporter
{
    // ─────────────────────────────────────────────────────────────────────────
    // 폴더 저장/불러오기 (EditorPrefs)
    private const string PrefKey = "StageAddressablesImporter.Folders";

    public static List<string> GetFolders()
    {
        var csv = EditorPrefs.GetString(PrefKey, "");
        var list = csv.Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (list.Count == 0) list.Add("Assets/Stages"); // 기본 폴더
        return list;
    }

    public static void SetFolders(IEnumerable<string> folders)
    {
        var norm = folders
            .Select(p => p.Replace('\\', '/'))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();
        EditorPrefs.SetString(PrefKey, string.Join("|", norm));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 그룹 이름 규칙: 레벨 범위/월드 등으로 샤딩
    private static string ResolveGroupName(int level)
    {
        int bucket = (level / 100) * 100; // 0,100,200...
        return $"Stages_{bucket:D4}_{bucket + 99:D4}";
    }

    /// <summary>
    /// 파일명 파싱
    /// stage_{level}_{variant}
    /// ex) stage_1125_3.json → level=1125, variant=3 (바리에이션 인덱스)
    /// </summary>
    /// <summary>
    /// 파일명 파싱
    /// stage_{level}_{variant}
    /// level_{level}
    /// level_{level}_{variant}
    /// ex) stage_1125_3.json → level=1125, variant=3
    /// ex) level_001.json   → level=1,   variant=0
    /// </summary>
    private static bool TryParse(string fileName, out int level, out int variant)
    {
        level = -1;
        variant = 0;   // 기본 variant = 0

        var parts = fileName.Split('_');

        // 허용 형태:
        //   stage_###_##
        //   level_###
        //   level_###_##
        if (parts.Length < 2 || parts.Length > 3)
            return false;

        // prefix: stage 또는 level
        if (parts[0] != "stage" && parts[0] != "level")
            return false;

        // levelId
        if (!int.TryParse(parts[1], out level))
            return false;

        // variant가 존재하면
        if (parts.Length == 3)
        {
            if (!int.TryParse(parts[2], out variant))
                return false;
        }

        return true;
    }



    private static AddressableAssetGroup CreateStageGroup(AddressableAssetSettings settings, string groupName)
    {
        var group = settings.CreateGroup(
            groupName, false, false, false, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

        var bundled = group.GetSchema<BundledAssetGroupSchema>();
        bundled.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel; // 라벨 단위 번들
        bundled.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;            // 런타임 로드 빠름
        // 버전에 따라 FullPath 같은 값이 없음 → 안정 선택
        bundled.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdHash;

        var update = group.GetSchema<ContentUpdateGroupSchema>();
        update.StaticContent = false; // 컨텐츠 업데이트 허용

        return group;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 실제 등록 로직 (메뉴 항목 제거, 창 버튼에서 호출)
   public static void RegisterAllStages()
{
    var settings = AddressableAssetSettingsDefaultObject.Settings;
    if (settings == null)
    {
        Debug.LogError("AddressableAssetSettings not found. Create Addressables settings first (Window > Asset Management > Addressables).");
        return;
    }

    var stageFolders = GetFolders();
    int processed = 0, updatedAddr = 0, newEntries = 0;

    foreach (var root in stageFolders)
    {
        if (!Directory.Exists(root)) continue;

        // stage_*.json + level_*.json 모두 스캔
        var files = Directory
            .GetFiles(root, "stage_*.json", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(root, "level_*.json", SearchOption.AllDirectories))
            .Distinct()
            .ToArray();

        foreach (var path in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(path); // stage_1125_3, level_001, level_001_2 ...
            if (!TryParse(fileName, out int level, out int variant))
            {
                Debug.LogWarning($"[StageImporter] Skip (name not match): {path}");
                continue;
            }

            // address: stages/{level}/{variant}
            string address = $"stages/{level}/{variant}";
            string levelLabel = $"stage-l{level}";
            string groupName = ResolveGroupName(level);

            var group = settings.FindGroup(groupName) ?? CreateStageGroup(settings, groupName);

            var guid = AssetDatabase.AssetPathToGUID(path);
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                newEntries++;
            }
            else if (entry.parentGroup != group)
            {
                settings.MoveEntry(entry, group);
            }

            if (entry.address != address)
            {
                entry.SetAddress(address);
                updatedAddr++;
            }

            entry.SetLabel(levelLabel, true, true); // 레벨 라벨
            entry.SetLabel("stage", true, true);    // 공통 라벨
            processed++;
        }
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Debug.Log($"[StageImporter] Done. processed={processed}, new={newEntries}, updatedAddr={updatedAddr}");
}


    // 폴더 관리 창 열기
    [MenuItem("Tools/Addressables/Stage Folders...")]
    public static void OpenFolderWindow() => StageAddressablesWindow.Open();
}

public class StageAddressablesWindow : EditorWindow
{
    Vector2 _scroll;
    List<string> _folders;

    public static void Open()
    {
        var w = GetWindow<StageAddressablesWindow>("Stage Folders");
        w.minSize = new Vector2(520, 320);
        w.Show();
    }

    void OnEnable()
    {
        _folders = StageAddressablesImporter.GetFolders();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Stage JSON Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("여기에 등록된 폴더들에서 stage_*.json을 찾아 Addressables에 자동 등록합니다.", MessageType.Info);

        // 드래그&드롭 영역
        var dropRect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
        GUI.Box(dropRect, "폴더 또는 Project의 폴더/파일을 여기로 드래그 앤 드롭");
        HandleDragAndDrop(dropRect);

        // 리스트
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        for (int i = 0; i < _folders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"• {_folders[i]}", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(_folders[i]);
                if (obj != null) EditorGUIUtility.PingObject(obj);
            }
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                _folders.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Folder...", GUILayout.Width(140)))
            {
                var path = EditorUtility.OpenFolderPanel("Select Stage Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    var rel = ToProjectRelativePath(path);
                    if (!string.IsNullOrEmpty(rel) && !_folders.Contains(rel))
                        _folders.Add(rel);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                StageAddressablesImporter.SetFolders(_folders);
                ShowNotification(new GUIContent("Saved"));
            }
            if (GUILayout.Button("Register Stages", GUILayout.Width(140)))
            {
                StageAddressablesImporter.SetFolders(_folders);
                StageAddressablesImporter.RegisterAllStages();
            }
        }
    }

    void HandleDragAndDrop(Rect rect)
    {
        var e = Event.current;
        if (!rect.Contains(e.mousePosition)) return;

        if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                // 1) Project 창에서 끌어온 경우(objectReferences)
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    var assetPath = AssetDatabase.GetAssetPath(obj);
                    if (string.IsNullOrEmpty(assetPath)) continue;

                    string add = assetPath;
                    if (File.Exists(assetPath)) add = Path.GetDirectoryName(assetPath)!.Replace('\\','/');

                    if (!string.IsNullOrEmpty(add) && Directory.Exists(add) && !_folders.Contains(add))
                        _folders.Add(add);
                }

                // 2) Finder/Explorer에서 끌어온 경우(paths: 절대 경로)
                foreach (var p in DragAndDrop.paths)
                {
                    string add = p.Replace('\\','/');
                    // 파일이면 상위 폴더 사용
                    if (File.Exists(add)) add = Path.GetDirectoryName(add)!.Replace('\\','/');

                    // 절대→프로젝트 상대 변환
                    var rel = ToProjectRelativePath(add);
                    if (!string.IsNullOrEmpty(rel) && Directory.Exists(rel) && !_folders.Contains(rel))
                        _folders.Add(rel);
                }
            }
            e.Use();
        }
    }

    static string ToProjectRelativePath(string absOrAssets)
    {
        if (string.IsNullOrEmpty(absOrAssets)) return null;

        // 이미 Assets/ 로 시작하면 그대로
        if (absOrAssets.StartsWith("Assets/") || absOrAssets == "Assets")
            return absOrAssets;

        var abs = absOrAssets.Replace('\\','/');
        var projAssets = Application.dataPath.Replace('\\','/'); // {Project}/Assets
        if (abs.StartsWith(projAssets))
            return "Assets" + abs.Substring(projAssets.Length);

        return null; // 프로젝트 외부 경로는 무시
    }
}
#endif
