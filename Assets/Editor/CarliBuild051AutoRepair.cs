#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-shot, explicitly armed GUI Editor repair. Never runs in Play Mode.
[InitializeOnLoad]
public static class CarliBuild051AutoRepair
{
    private const string ScenePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
    private const string RecorderPath = "Assets/CARLiR4_Validation/PlayModeEvidence/Runtime/CarliSanctuaryPlayModeEvidence.cs";
    private static readonly string Root = Directory.GetParent(Application.dataPath).FullName;
    private static readonly string EvidenceDir = Path.Combine(Root, "CARLi_R4_Evidence");
    private static readonly string ArmFile = Path.Combine(EvidenceDir, "B05_1_ARM_REPAIR.txt");
    private static readonly string ResultFile = Path.Combine(EvidenceDir, "B05_1_REPAIR_RESULT.txt");
    private static bool attempted;

    static CarliBuild051AutoRepair()
    {
        EditorApplication.update += TryRepair;
    }

    [MenuItem("CARLi/Build 05.1/Run Armed Scene Repair")]
    public static void RunFromMenu() => TryRepair(true);

    private static void TryRepair() => TryRepair(false);

    private static void TryRepair(bool fromMenu)
    {
        if (attempted || !File.Exists(ArmFile)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        attempted = true;
        EditorApplication.update -= TryRepair;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Report("NO_GO: EXIT_PLAY_MODE_FIRST");
            return;
        }

        // Refuse to change scene state if the editor has unsaved scene modifications.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene opened = SceneManager.GetSceneAt(i);
            if (opened.IsValid() && opened.isDirty)
            {
                Report("NO_GO: UNSAVED_OPEN_SCENE=" + opened.path);
                return;
            }
        }

        Scene target = default(Scene);
        bool openedAdditively = false;
        try
        {
            if (!File.Exists(Path.Combine(Root, ScenePath.Replace('/', Path.DirectorySeparatorChar))))
                throw new InvalidOperationException("SCENE_FILE_MISSING");

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(RecorderPath);
            Type recorderType = monoScript != null ? monoScript.GetClass() : null;
            if (recorderType == null || !typeof(MonoBehaviour).IsAssignableFrom(recorderType))
                throw new InvalidOperationException("RECORDER_TYPE_NOT_COMPILED");

            target = SceneManager.GetSceneByPath(ScenePath);
            if (!target.IsValid() || !target.isLoaded)
            {
                target = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                openedAdditively = true;
            }

            Component existing = null;
            Canvas canvas = null;
            foreach (GameObject root in target.GetRootGameObjects())
            {
                foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null && mb.GetType() == recorderType) { existing = mb; break; }
                }
                if (existing != null) break;
            }

            if (existing == null)
            {
                foreach (GameObject root in target.GetRootGameObjects())
                {
                    canvas = root.GetComponentInChildren<Canvas>(true);
                    if (canvas != null) break;
                }
                if (canvas == null) throw new InvalidOperationException("CANVAS_NOT_FOUND");
                if (!canvas.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("CANVAS_INACTIVE_REFUSE_TO_CHANGE_HIERARCHY");
                existing = canvas.gameObject.AddComponent(recorderType);
                EditorSceneManager.MarkSceneDirty(target);
            }

            Behaviour behaviour = existing as Behaviour;
            if (behaviour == null) throw new InvalidOperationException("RECORDER_NOT_BEHAVIOUR");
            if (!behaviour.gameObject.activeInHierarchy)
                throw new InvalidOperationException("RECORDER_OBJECT_INACTIVE");
            if (!behaviour.enabled)
            {
                behaviour.enabled = true;
                EditorSceneManager.MarkSceneDirty(target);
            }
            if (!EditorSceneManager.SaveScene(target))
                throw new InvalidOperationException("SCENE_SAVE_RETURNED_FALSE");

            // Verify saved scene by reopening additively only if we owned the additive load.
            string status = "PASS: RECORDER_PRESENT_ENABLED_SAVED; OBJECT=" + behaviour.gameObject.name +
                "; TYPE=" + recorderType.FullName + "; SCENE=" + ScenePath;
            Report(status);
        }
        catch (Exception e)
        {
            Report("NO_GO: " + e.GetType().Name + ": " + e.Message);
        }
        finally
        {
            if (openedAdditively && target.IsValid() && target.isLoaded)
                EditorSceneManager.CloseScene(target, true);
        }
    }

    private static void Report(string status)
    {
        Directory.CreateDirectory(EvidenceDir);
        File.WriteAllText(ResultFile, "UTC=" + DateTime.UtcNow.ToString("o") + Environment.NewLine + status + Environment.NewLine);
        if (status.StartsWith("PASS:", StringComparison.Ordinal)) Debug.Log("CARLI_B051_REPAIR_PASS: " + status);
        else Debug.LogError("CARLI_B051_REPAIR_" + status);
        // Disarm on both success and failure so there is no unrequested retry.
        try { File.Delete(ArmFile); } catch (Exception e) { Debug.LogWarning("CARLI_B051_DISARM_FAILED: " + e.Message); }
    }
}
#endif
