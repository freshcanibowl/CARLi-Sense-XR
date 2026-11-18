using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class B054AddOvrRig
{
    public static void Run()
    {
        const string targetPath =
            "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity";
        var scene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);

        // Resolve OVR types from the just-imported Meta XR package.
        var rigType = System.Type.GetType(
            "OVRCameraRig, Oculus.VR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        if (rigType == null)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "Oculus.VR")
                {
                    rigType = asm.GetType("OVRCameraRig");
                    break;
                }
            }
        }
        if (rigType == null)
        {
            Debug.LogError("B054_FAIL: OVRCameraRig type not found (Meta XR SDK not compiled)");
            EditorApplication.Exit(3);
            return;
        }

        var existing = Object.FindAnyObjectByType(rigType, FindObjectsInactive.Include);
        if (existing != null)
        {
            Debug.Log("B054_RIG_ALREADY_PRESENT=" + existing.name);
        }
        else
        {
            // OVRCameraRig ships as a prefab inside the package; instantiate it.
            var guids = AssetDatabase.FindAssets("OVRCameraRig t:Prefab");
            GameObject rig = null;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.GetComponent(rigType) != null)
                {
                    rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("B054_RIG_INSTANTIATED_FROM=" + path);
                    break;
                }
            }
            if (rig == null)
            {
                // Fallback: build a minimal rig with OVRManager so the feature config exists.
                var go = new GameObject("OVRCameraRig");
                go.AddComponent(rigType);
                rig = go;
                Debug.Log("B054_RIG_BUILT_MINIMAL");
            }
            rig.transform.position = new Vector3(0f, 0f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, targetPath);
        Debug.Log("B054_SAVED=" + saved);
        EditorApplication.Exit(saved ? 0 : 6);
    }
}
