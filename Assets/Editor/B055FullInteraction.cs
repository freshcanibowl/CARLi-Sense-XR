using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class B055FullInteraction
{
    private static bool _entered;
    private static float _startTime;
    private static int _index;
    private static float _nextClickTime;
    private static int _clicked;
    private static bool _verifying;
    private static float _verifyStartTime;

    private static readonly string[] All = {
        "01 Observe", "02 Context", "03 Respond", "04 Consent", "05 Journal", "06 Finish",
        "English", "中文", "BM",
        "Observation 1", "Observation 2", "Observation 3", "Observation 4"
    };

    public static void Run()
    {
        EditorSceneManager.OpenScene(
            "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity",
            OpenSceneMode.Single);
        EditorApplication.update += Poll;
        EditorApplication.EnterPlaymode();
    }

    private static void Poll()
    {
        if (!EditorApplication.isPlaying) return;
        if (!_entered)
        {
            _entered = true;
            _startTime = (float)EditorApplication.timeSinceStartup;
            _nextClickTime = _startTime + 1f; // let the recorder's Start() settle
            return;
        }

        float now = (float)EditorApplication.timeSinceStartup;

        if (_verifying)
        {
            if (now - _verifyStartTime > 10f)
            {
                Debug.Log("B055_OK FAIL TIMEOUT_VERIFYING");
                EditorApplication.Exit(10);
            }
            return;
        }

        if (now < _nextClickTime) return;

        if (_index >= All.Length)
        {
            _verifying = true;
            _verifyStartTime = now;
            return;
        }

        string name = All[_index];
        var go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogError("B055_BUTTON_MISSING:" + name);
        }
        else
        {
            var button = go.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("B055_NO_BUTTON:" + name);
            }
            else
            {
                // Fires every onClick listener (persistent + non-persistent), the
                // same set a real pointer click would invoke on the Button.
                button.onClick.Invoke();
                _clicked++;
            }
        }
        _index++;
        _nextClickTime = now + 0.1f;
        if (_index >= All.Length)
        {
            Debug.Log("B055_CLICKED_COUNT=" + _clicked + "/13");
            _verifying = true;
            _verifyStartTime = now;
        }
    }

    static B055FullInteraction()
    {
        // Verify after clicks settle; run on a periodic editor callback.
        EditorApplication.update += VerifyPoll;
    }

    private static void VerifyPoll()
    {
        if (!_verifying || !EditorApplication.isPlaying) return;
        var receipt = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            "CARLi_R4_Evidence", "R5_Build05_PlayModeEvidence.json");
        if (!File.Exists(receipt)) return;
        string json = File.ReadAllText(receipt);
        // A click count above zero proves interactions landed; wait until stable.
        var marker = "\"actualButtonClicks\": " + _clicked;
        if (json.Contains(marker) && json.Contains("actions"))
        {
            Debug.Log("B055_RECEIPT=" + json);
            bool pass = json.Contains("PASS_EDITOR_PLAYMODE_UI_ONLY");
            Debug.Log(pass ? "B055_OK PASS" : "B055_OK FAIL");
            _verifying = false;
            EditorApplication.Exit(pass ? 0 : 9);
        }
    }
}
