#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CarliSenseXR.Sanctuary;
using CarliSenseXR.Sanctuary.Evidence;

namespace CarliSenseXR.Sanctuary.Evidence.EditorTools
{
    public static class CarliBuild05AttachRecorder
    {
        private const string ScenePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
        [MenuItem("CARLi/Build 05/Attach Play Mode Evidence Recorder")]
        public static void Attach()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            { Debug.LogError("CARLI_BUILD05_SCENE_MISSING"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CarliSanctuaryPreview preview = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                preview = root.GetComponentInChildren<CarliSanctuaryPreview>(true);
                if (preview != null) break;
            }
            if (preview == null) { Debug.LogError("CARLI_BUILD05_PREVIEW_MISSING"); return; }
            var canvasRoot = preview.GetComponentInParent<Canvas>();
            if (canvasRoot == null) { Debug.LogError("CARLI_BUILD05_CANVAS_MISSING"); return; }
            if (canvasRoot.GetComponent<CarliSanctuaryPlayModeEvidence>() == null)
                canvasRoot.gameObject.AddComponent<CarliSanctuaryPlayModeEvidence>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("CARLI_BUILD05_RECORDER_ATTACHED=PASS_SCENE_READY_FOR_MANUAL_PLAY_MODE");
        }
    }
}
#endif
