#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicitly armed, one-shot GUI repair; no headless Unity licensing required.
[InitializeOnLoad]
public static class CarliBuild052Repair
{
    private const string ScenePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
    private const string RecorderPath = "Assets/CARLiR4_Validation/PlayModeEvidence/Runtime/CarliSanctuaryPlayModeEvidence.cs";
    private static readonly string Root = Directory.GetParent(Application.dataPath).FullName;
    private static readonly string EvidenceDir = Path.Combine(Root, "CARLi_R4_Evidence");
    private static readonly string Arm = Path.Combine(EvidenceDir, "B05_2_ARM_REPAIR.txt");
    private static readonly string Result = Path.Combine(EvidenceDir, "B05_2_REPAIR_RESULT.txt");
    private static bool attempted;

    static CarliBuild052Repair() { EditorApplication.update += AttemptFromUpdate; }
    private static void AttemptFromUpdate() { Attempt(); }

    [MenuItem("CARLi/Build 05.2/Run Armed Recorder Repair")]
    public static void RunArmedRepair() { Attempt(); }

    private static void Attempt()
    {
        if (attempted || !File.Exists(Arm) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        attempted = true;
        EditorApplication.update -= AttemptFromUpdate;
        Scene target = default(Scene);
        bool addedScene = false;
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("EXIT_PLAY_MODE_FIRST");
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.IsValid() && s.isDirty) throw new InvalidOperationException("UNSAVED_OPEN_SCENE=" + s.path);
            }
            if (!File.Exists(Path.Combine(Root, ScenePath.Replace('/', Path.DirectorySeparatorChar))))
                throw new InvalidOperationException("SCENE_FILE_MISSING");
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(RecorderPath);
            if (script == null) throw new InvalidOperationException("RECORDER_MONOSCRIPT_MISSING");
            Type type = script.GetClass();
            if (type == null) throw new InvalidOperationException("RECORDER_CLASS_NOT_COMPILED");
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                throw new InvalidOperationException("RECORDER_NOT_MONOBEHAVIOUR: " + type.FullName);
            if (type.IsAbstract || type.ContainsGenericParameters)
                throw new InvalidOperationException("RECORDER_NOT_INSTANTIABLE: " + type.FullName);

            target = SceneManager.GetSceneByPath(ScenePath);
            if (!target.IsValid() || !target.isLoaded)
            {
                target = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                addedScene = true;
            }
            MonoBehaviour recorder = null;
            Canvas canvas = null;
            foreach (GameObject root in target.GetRootGameObjects())
            {
                foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null && mb.GetType() == type) { recorder = mb; break; }
                }
                if (recorder != null) break;
            }
            bool created = false;
            if (recorder == null)
            {
                foreach (GameObject root in target.GetRootGameObjects())
                {
                    canvas = root.GetComponentInChildren<Canvas>(true);
                    if (canvas != null) break;
                }
                if (canvas == null) throw new InvalidOperationException("CANVAS_NOT_FOUND");
                if (!canvas.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("CANVAS_INACTIVE");
                // Undo.AddComponent is the Editor-aware path. Verify return value BEFORE accessing it.
                Component added = Undo.AddComponent(canvas.gameObject, type);
                if (added == null)
                    throw new InvalidOperationException("ADD_COMPONENT_RETURNED_NULL; TYPE=" + type.FullName + "; OBJECT=" + canvas.gameObject.name + "; CHECK_UNITY_CONSOLE");
                recorder = added as MonoBehaviour;
                if (recorder == null)
                    throw new InvalidOperationException("ADD_COMPONENT_UNEXPECTED_RUNTIME_TYPE=" + added.GetType().FullName);
                created = true;
            }
            if (recorder == null) throw new InvalidOperationException("RECORDER_REFERENCE_INVALID_AFTER_ATTACH");
            if (!recorder.gameObject.activeInHierarchy) throw new InvalidOperationException("RECORDER_GAMEOBJECT_INACTIVE");
            if (!recorder.enabled) recorder.enabled = true;
            EditorUtility.SetDirty(recorder);
            EditorSceneManager.MarkSceneDirty(target);
            if (!EditorSceneManager.SaveScene(target)) throw new InvalidOperationException("SCENE_SAVE_FAILED");
            string detail = "PASS: RECORDER_PRESENT_ENABLED_SAVED" + Environment.NewLine +
                "CREATED=" + created + Environment.NewLine + "TYPE=" + type.FullName + Environment.NewLine +
                "OBJECT=" + recorder.gameObject.name + Environment.NewLine + "SCENE=" + ScenePath;
            Report(detail);
        }
        catch (Exception ex)
        {
            Report("NO_GO: " + ex.GetType().Name + ": " + ex.Message);
        }
        finally
        {
            if (addedScene && target.IsValid() && target.isLoaded)
            {
                // Don't discard unsaved edits in case the save/attach path failed.
                if (!target.isDirty) EditorSceneManager.CloseScene(target, true);
                else Debug.LogWarning("CARLI_B052_SCENE_LEFT_OPEN_DIRTY_FOR_INSPECTION");
            }
        }
    }

    private static void Report(string status)
    {
        Directory.CreateDirectory(EvidenceDir);
        File.WriteAllText(Result, "UTC=" + DateTime.UtcNow.ToString("o") + Environment.NewLine + status + Environment.NewLine);
        if (status.StartsWith("PASS:", StringComparison.Ordinal)) Debug.Log("CARLI_B052_REPAIR_PASS " + status);
        else Debug.LogError("CARLI_B052_REPAIR_NO_GO " + status);
        try { File.Delete(Arm); }
        catch (Exception e) { Debug.LogWarning("CARLI_B052_DISARM_ERROR " + e.Message); }
    }
}
#endif
