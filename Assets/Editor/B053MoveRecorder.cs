using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CarliSenseXR.Sanctuary;
using CarliSenseXR.Sanctuary.Evidence;

public static class B053MoveRecorder
{
    public static void Run()
    {
        const string targetPath =
            "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity";
        var scene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);

        var recorder = Object.FindAnyObjectByType<CarliSanctuaryPlayModeEvidence>(FindObjectsInactive.Include);
        if (recorder == null) { Debug.LogError("B053_MOVE_FAIL: recorder missing"); EditorApplication.Exit(3); return; }

        var oldHost = recorder.gameObject;
        var canvas = oldHost.GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogError("B053_MOVE_FAIL: no canvas parent"); EditorApplication.Exit(4); return; }
        var newHost = canvas.gameObject;
        if (newHost == oldHost) { Debug.Log("B053_MOVE_ALREADY_ON_ROOT"); EditorApplication.Exit(0); return; }

        Object.DestroyImmediate(recorder);
        newHost.AddComponent<CarliSanctuaryPlayModeEvidence>();
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, targetPath);
        Debug.Log("B053_MOVE_SAVED=" + saved + " newHost=" + newHost.name);

        var guid = AssetDatabase.AssetPathToGUID(
            "Assets/CARLiR4_Validation/PlayModeEvidence/Runtime/CarliSanctuaryPlayModeEvidence.cs");
        int refs = System.IO.File.ReadAllText(targetPath).Split(new[] { guid },
            System.StringSplitOptions.None).Length - 1;
        Debug.Log("B053_MOVE_REFERENCES=" + refs);
        EditorApplication.Exit(refs > 0 ? 0 : 6);
    }
}
