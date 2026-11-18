#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CarliSenseXR.Sanctuary;

namespace CarliSenseXR.Sanctuary.EditorTools
{
    // Read-only audit of the Build03 desktop preview. No consent / Journal / XR claims.
    public static class CarliSanctuarySceneAudit
    {
        private const string SceneAsset = "Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_Preview.unity";
        private static readonly string[] StepNames = { "01 Observe", "02 Context", "03 Respond", "04 Consent", "05 Journal", "06 Finish" };
        private static readonly string[] LanguageNames = { "English", "中文", "BM" };
        private static readonly string[] RefNames = { "titleLabel", "bodyLabel", "comfortLabel", "privacyLabel", "observationLabel", "stateLabel" };

        [Serializable]
        private sealed class AuditReceipt
        {
            public string status, scene, executedAtUtc, disclaimer, scope;
            public int sceneButtonCount, checkedEventCount;
            public string[] failures, warnings;
        }

        [MenuItem("CARLi/Build 04/Audit Care Sanctuary Preview Scene")]
        public static void RunFromMenu() { RunAudit(); }

        // For -executeMethod CarliSenseXR.Sanctuary.EditorTools.CarliSanctuarySceneAudit.RunAudit
        public static void RunAudit()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var report = new AuditReceipt {
                scene = SceneAsset,
                executedAtUtc = DateTime.UtcNow.ToString("o"),
                scope = "Editor desktop preview wiring only; no XR tracking, real consent, data export, runtime emotional outcomes"
            };
            Scene opened = default(Scene);
            try
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAsset) == null)
                {
                    errors.Add("SCENE_NOT_FOUND: create via CARLi > Build 03 > Create Care Sanctuary Preview Scene");
                }
                else
                {
                    // Additive inspection avoids losing an already-open/unsaved user scene.
                    opened = EditorSceneManager.OpenScene(SceneAsset, OpenSceneMode.Additive);
                    var buttons = new Dictionary<string, Button>();
                    CarliSanctuaryPreview preview = null;
                    bool hasEventSystem = false, hasCanvas = false;
                    foreach (var root in opened.GetRootGameObjects())
                    {
                        if (root.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true) != null) hasEventSystem = true;
                        if (root.GetComponentInChildren<Canvas>(true) != null) hasCanvas = true;
                        if (preview == null) preview = root.GetComponentInChildren<CarliSanctuaryPreview>(true);
                        foreach (var button in root.GetComponentsInChildren<Button>(true))
                        {
                            if (buttons.ContainsKey(button.gameObject.name)) errors.Add("DUPLICATE_BUTTON_NAME: " + button.gameObject.name);
                            else buttons.Add(button.gameObject.name, button);
                        }
                    }
                    if (!hasEventSystem) errors.Add("EVENT_SYSTEM_MISSING");
                    if (!hasCanvas) errors.Add("CANVAS_MISSING");
                    if (preview == null) errors.Add("PREVIEW_CONTROLLER_MISSING");
                    report.sceneButtonCount = buttons.Count;

                    if (preview != null)
                    {
                        var so = new SerializedObject(preview);
                        foreach (var field in RefNames)
                        {
                            var property = so.FindProperty(field);
                            if (property == null || property.objectReferenceValue == null)
                                errors.Add("UNBOUND_PREVIEW_LABEL: " + field);
                        }
                        // Static preview banner must explicitly disclose lack of consent / export.
                        var banner = so.FindProperty("stateLabel");
                        if (banner != null && banner.objectReferenceValue is Text text)
                            report.disclaimer = text.text;
                        // The controller sets the final disclaimer during Play Mode; audit that it exists in code via runtime inspection separately.
                        if (banner != null && banner.objectReferenceValue == null) errors.Add("PREVIEW_DISCLAIMER_LABEL_UNBOUND");
                    }

                    foreach (string name in StepNames) CheckButton(buttons, name, "SetStep", preview, errors, ref report.checkedEventCount);
                    foreach (string name in LanguageNames) CheckButton(buttons, name, "SetLanguage", preview, errors, ref report.checkedEventCount);
                    for (int i = 1; i <= 4; i++) CheckButton(buttons, "Observation " + i, "SelectObservation", preview, errors, ref report.checkedEventCount);
                    if (buttons.Count != 13) warnings.Add("EXPECTED_13_BUTTONS_FOUND_" + buttons.Count);
                    warnings.Add("INSPECT_IN_PLAY_MODE: Chinese glyphs, layout, legibility and language action text");
                    warnings.Add("NOT_PROVEN: real XR hand tracking, consent, Journal export, user emotional value");
                }
            }
            catch (Exception e) { errors.Add("AUDIT_EXCEPTION: " + e.GetType().Name + " " + e.Message); }
            finally
            {
                if (opened.IsValid() && opened.isLoaded) EditorSceneManager.CloseScene(opened, true);
            }
            report.failures = errors.ToArray();
            report.warnings = warnings.ToArray();
            report.status = errors.Count == 0 ? "PASS_PREVIEW_WIRING_ONLY" : "NO_GO";
            string evidenceDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CARLi_R4_Evidence");
            Directory.CreateDirectory(evidenceDir);
            string path = Path.Combine(evidenceDir, "R5_Build04_SceneAudit.json");
            File.WriteAllText(path, JsonUtility.ToJson(report, true), new System.Text.UTF8Encoding(false));
            Debug.Log("CARLI_BUILD04_AUDIT=" + report.status + " CHECKED_EVENTS=" + report.checkedEventCount + " FAILURES=" + errors.Count + " RECEIPT=" + path);
            if (errors.Count > 0) foreach (var issue in errors) Debug.LogError("CARLI_BUILD04: " + issue);
        }

        private static void CheckButton(Dictionary<string, Button> buttons, string name, string method,
            CarliSanctuaryPreview preview, List<string> errors, ref int checkedCount)
        {
            if (!buttons.TryGetValue(name, out var button)) { errors.Add("BUTTON_MISSING: " + name); return; }
            if (button.onClick.GetPersistentEventCount() != 1)
            { errors.Add("UNEXPECTED_EVENT_COUNT: " + name + " = " + button.onClick.GetPersistentEventCount()); return; }
            var target = button.onClick.GetPersistentTarget(0);
            var boundMethod = button.onClick.GetPersistentMethodName(0);
            if (target != preview || boundMethod != method)
            { errors.Add("WRONG_EVENT_WIRING: " + name + " -> " + boundMethod); return; }
            checkedCount++;
        }
    }
}
#endif
