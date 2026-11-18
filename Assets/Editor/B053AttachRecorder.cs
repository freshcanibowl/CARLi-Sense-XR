using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CarliSenseXR.Sanctuary;
using CarliSenseXR.Sanctuary.Evidence;

public static class B053AttachRecorder
{
    public static void Run()
    {
        const string sourcePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
        const string targetPath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity";

        var scene = EditorSceneManager.OpenScene(sourcePath, OpenSceneMode.Single);

        // Avoid duplicating the recorder if it somehow already exists in the saved scene.
        var existing = Object.FindObjectOfType<CarliSanctuaryPlayModeEvidence>(true);
        if (existing == null)
        {
            var preview = Object.FindObjectOfType<CarliSanctuaryPreview>(true);
            if (preview == null)
            {
                Debug.LogError("B053_FAIL: CarliSanctuaryPreview not found in scene");
                EditorApplication.Exit(3);
                return;
            }
            var host = preview.gameObject;
            var recorder = host.AddComponent<CarliSanctuaryPlayModeEvidence>();
            if (recorder == null)
            {
                Debug.LogError("B053_FAIL: AddComponent returned null");
                EditorApplication.Exit(4);
                return;
            }
            Debug.Log("B053_RECORDER_ATTACHED_TO=" + host.name);
        }
        else
        {
            Debug.Log("B053_RECORDER_ALREADY_PRESENT_ON=" + existing.gameObject.name);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, targetPath);
        if (!saved)
        {
            Debug.LogError("B053_FAIL: SaveScene returned false for " + targetPath);
            EditorApplication.Exit(5);
            return;
        }

        // Read back from disk to prove persistence.
        var savedText = System.IO.File.ReadAllText(targetPath);
        var guid = AssetDatabase.AssetPathToGUID(
            "Assets/CARLiR4_Validation/PlayModeEvidence/Runtime/CarliSanctuaryPlayModeEvidence.cs");
        int refs = 0;
        int idx = 0;
        while ((idx = savedText.IndexOf(guid, idx, System.StringComparison.Ordinal)) >= 0) { refs++; idx++; }
        Debug.Log("B053_SAVED_SCENE=" + targetPath);
        Debug.Log("B053_RECORDER_REFERENCES=" + refs);
        Debug.Log("B053_OK refs=" + refs + (refs > 0 ? " PASS" : " FAIL"));
        EditorApplication.Exit(refs > 0 ? 0 : 6);
    }
}
