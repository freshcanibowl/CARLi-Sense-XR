#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using CarliSenseXR.Sanctuary;

namespace CarliSenseXR.Sanctuary.EditorTools
{
    public static class CreateCarliSanctuaryScene
    {
        private const string ScenePath = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
        private static Font font;
        private static readonly Color Ink = new Color(.17f, .24f, .25f);
        private static readonly Color Cream = new Color(.97f, .95f, .91f);
        private static readonly Color Teal = new Color(.15f, .42f, .42f);
        private static readonly Color Amber = new Color(.79f, .50f, .23f);

        [MenuItem("CARLi/Build 03/Create Care Sanctuary Preview Scene")]
        public static void Create()
        {
            // This intentionally creates a separate preview scene, not a production XR scene.
            if (System.IO.File.Exists(ScenePath) && !EditorUtility.DisplayDialog("CARLi scene exists",
                "Replace the existing PREVIEW scene? This will overwrite changes to it.", "Replace preview", "Cancel")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var fontNames = new[] { "Microsoft YaHei", "Arial", "Liberation Sans" };
            font = Font.CreateDynamicFontFromOSFont(fontNames, 26);
            var root = new GameObject("CARE SANCTUARY — EDITOR PREVIEW ONLY", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);

            var background = Panel("Warm backdrop", root.transform, new Color(.95f, .95f, .91f), 0, 0, 1600, 900);
            background.transform.SetAsFirstSibling();
            Panel("Header backdrop", root.transform, Teal, 0, 380, 1600, 140);
            Text("CARLi  ·  Care Sanctuary", root.transform, 40, Color.white, -420, 415, 700, 60);
            Text("每一个小瞬间，都值得被看见  |  Every little moment matters", root.transform, 23, Color.white, -360, 365, 980, 45);

            var card = Panel("Care moment card", root.transform, Color.white, -130, 20, 1140, 560);
            var preview = card.AddComponent<CarliSanctuaryPreview>();
            var title = Text("Moment title", card.transform, 38, Ink, 0, 202, 960, 60);
            var body = Text("Body", card.transform, 25, Ink, 0, 112, 965, 105);
            var comfort = Text("Supportive note", card.transform, 25, Teal, 0, 0, 965, 95);
            var privacy = Text("Privacy boundary", card.transform, 20, new Color(.45f,.45f,.45f), 0, -104, 965, 85);
            var observed = Text("Observed behavior", card.transform, 20, Amber, 0, -206, 965, 65);
            var state = Text("PREVIEW MODE", root.transform, 18, new Color(.45f,.32f,.21f), -110, -395, 1120, 40);
            Assign(preview, "titleLabel", title); Assign(preview, "bodyLabel", body);
            Assign(preview, "comfortLabel", comfort); Assign(preview, "privacyLabel", privacy);
            Assign(preview, "observationLabel", observed); Assign(preview, "stateLabel", state);

            // Step navigation is only for copy walkthrough, never controls workflow state.
            string[] steps = { "01 Observe", "02 Context", "03 Respond", "04 Consent", "05 Journal", "06 Finish" };
            for (int i=0; i<6; i++)
            {
                var button = Button(steps[i], root.transform, -640 + i*225, -302, 205, 54, Teal);
                UnityEventTools.AddIntPersistentListener(button.onClick, preview.SetStep, i);
            }
            string[] langs = { "English", "中文", "BM" };
            for (int i=0; i<3; i++)
            {
                var button = Button(langs[i], root.transform, 410 + i*145, 407, 128, 44, Amber);
                UnityEventTools.AddIntPersistentListener(button.onClick, preview.SetLanguage, i);
            }
            // Selection deliberately records no data; it updates only this scene's transient label.
            for (int i=0; i<4; i++)
            {
                var button = Button("Observation " + (i+1), root.transform, -485 + i*260, -371, 242, 45, new Color(.62f,.67f,.62f));
                UnityEventTools.AddIntPersistentListener(button.onClick, preview.SelectObservation, i);
            }
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            if (!AssetDatabase.IsValidFolder("Assets/CARLiR4_Validation/Sanctuary/Scenes"))
                AssetDatabase.CreateFolder("Assets/CARLiR4_Validation/Sanctuary", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = root;
            Debug.Log("CARLI_BUILD03_PREVIEW_SCENE_CREATED=" + ScenePath + " (no consent / export / XR interaction)");
        }
        private static void Assign(Object target, string field, Text value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static GameObject Panel(string name, Transform parent, Color color, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.sizeDelta=new Vector2(w,h); rt.anchoredPosition=new Vector2(x,y);
            var graphic=go.GetComponent<Image>(); graphic.color=color; graphic.raycastTarget=false;
            return go;
        }
        private static Text Text(string name, Transform parent, int size, Color color, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.sizeDelta=new Vector2(w,h); rt.anchoredPosition=new Vector2(x,y);
            var t=go.GetComponent<Text>(); t.font=font; t.fontSize=size; t.color=color;
            t.alignment=TextAnchor.MiddleCenter; t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Overflow;
            t.text=name; t.raycastTarget=false; return t;
        }
        private static Button Button(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var go = Panel(name, parent, color, x, y, w, h);
            go.GetComponent<Image>().raycastTarget=true;
            var btn=go.AddComponent<Button>();
            Text(name + " label",go.transform, 18, Color.white, 0, 0, w-12, h-5).text=name;
            return btn;
        }
    }
}
#endif
