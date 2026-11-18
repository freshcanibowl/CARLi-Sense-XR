using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CarliBuild05SceneRepair
{
    [MenuItem("CARLi/Build 05/Repair Recorder Scene")]
    public static void Repair()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("CARLI_B05_REPAIR_NO_GO: EXIT_PLAY_MODE_FIRST");
            return;
        }

        const string path =
            "Assets/CARLiR4_Validation/Sanctuary/Scenes/" +
            "CARLi_CareSanctuary_Preview.unity";

        Scene scene = EditorSceneManager.OpenScene(
            path, OpenSceneMode.Single);

        var recorderType = Type.GetType(
            "CarliSenseXR.Sanctuary.Evidence." +
            "CarliSanctuaryPlayModeEvidence, Assembly-CSharp");

        if (recorderType == null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                recorderType = assembly.GetType(
                    "CarliSenseXR.Sanctuary.Evidence." +
                    "CarliSanctuaryPlayModeEvidence");

                if (recorderType != null) break;
            }
        }

        if (recorderType == null ||
            !typeof(MonoBehaviour).IsAssignableFrom(recorderType))
        {
            Debug.LogError(
                "CARLI_B05_REPAIR_NO_GO: RECORDER_TYPE_NOT_FOUND");
            return;
        }

        MonoBehaviour recorder = null;
        GameObject canvas = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var component in
                root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component != null &&
                    component.GetType() == recorderType)
                {
                    recorder = component;
                    break;
                }
            }

            if (recorder != null) break;
        }

        if (recorder == null)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var c in
                    root.GetComponentsInChildren<Canvas>(true))
                {
                    canvas = c.gameObject;
                    break;
                }

                if (canvas != null) break;
            }

            if (canvas == null)
            {
                Debug.LogError(
                    "CARLI_B05_REPAIR_NO_GO: CANVAS_NOT_FOUND");
                return;
            }

            recorder = (MonoBehaviour)
                canvas.AddComponent(recorderType);
        }

        recorder.gameObject.SetActive(true);
        recorder.enabled = true;

        EditorSceneManager.MarkSceneDirty(scene);

        if (!EditorSceneManager.SaveScene(scene))
        {
            Debug.LogError(
                "CARLI_B05_REPAIR_NO_GO: SCENE_SAVE_FAILED");
            return;
        }

        Debug.Log(
            "CARLI_B05_REPAIR_SAVED: " +
            recorder.gameObject.name);
    }
}
