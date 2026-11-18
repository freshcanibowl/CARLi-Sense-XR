using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class B053PlayModeCheck
{
    private static bool _entered;
    private static float _playEnteredTime;
    private const float TimeoutSeconds = 30f;

    public static void Run()
    {
        const string scenePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        EditorApplication.update += Poll;
        EditorApplication.EnterPlaymode();
    }

    private static void Poll()
    {
        if (!EditorApplication.isPlaying) return;
        if (!_entered)
        {
            _entered = true;
            _playEnteredTime = (float)EditorApplication.timeSinceStartup;
        }
        if ((float)EditorApplication.timeSinceStartup - _playEnteredTime > TimeoutSeconds)
        {
            Debug.Log("B053_PLAY_OK FAIL TIMEOUT_NO_RECEIPT");
            EditorApplication.Exit(8);
            return;
        }
        var receipt = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            "CARLi_R4_Evidence", "R5_Build05_PlayModeEvidence.json");
        if (File.Exists(receipt))
        {
            string json = File.ReadAllText(receipt);
            Debug.Log("B053_PLAY_RECEIPT_EXISTS=True");
            Debug.Log("B053_PLAY_RECEIPT_CONTENT=" + json);
            Debug.Log("B053_PLAY_OK PASS");
            EditorApplication.Exit(0);
        }
    }
}
