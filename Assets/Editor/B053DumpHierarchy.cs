using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class B053DumpHierarchy
{
    public static void Run()
    {
        EditorSceneManager.OpenScene(
            "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity",
            OpenSceneMode.Single);
        Dump(null, 0);
        EditorApplication.Exit(0);
    }

    private static void Dump(Transform t, int depth)
    {
        var go = t == null ? null : t.gameObject;
        var name = go == null ? "<root>" : go.name;
        var comps = go == null ? "" : string.Join(",", System.Array.ConvertAll(go.GetComponents<Component>(), c => c == null ? "null" : c.GetType().Name));
        Debug.Log("HIER " + new string(' ', depth * 2) + name + " [" + comps + "]");
        if (t == null)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()) Dump(root.transform, 1);
            return;
        }
        for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1);
    }
}
