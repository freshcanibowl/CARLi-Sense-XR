using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using CarliSenseXR.CareMoments;
using CarliSenseXR.Sanctuary;

namespace CarliSenseXR.Sanctuary.Evidence
{
    // Only records real Editor Play Mode UI clicks. Never sends data anywhere.
    // No consent, XR tracking, Journal, or private pet information is accessed.
    public sealed class CarliSanctuaryPlayModeEvidence : MonoBehaviour
    {
        [Serializable]
        private sealed class Receipt
        {
            public string status = "NO_GO";
            public string scope = "Editor Play Mode UI click evidence only; not XR, consent, export or emotional impact";
            public string startedAtUtc;
            public string updatedAtUtc;
            public int actualButtonClicks;
            public int uniqueSteps;
            public int uniqueLanguages;
            public int uniqueObservations;
            public int labelValidationFailures;
            public bool previewDisclaimerVisible;
            public string[] failures = new string[0];
            public string[] actions = new string[0];
        }

        private static readonly string[] Steps = { "01 Observe", "02 Context", "03 Respond", "04 Consent", "05 Journal", "06 Finish" };
        private static readonly string[] Languages = { "English", "中文", "BM" };
        private static readonly string[] Observations = { "Observation 1", "Observation 2", "Observation 3", "Observation 4" };
        private readonly bool[] seenSteps = new bool[6], seenLanguages = new bool[3], seenObservations = new bool[4];
        private readonly List<string> failures = new List<string>();
        private readonly List<string> actions = new List<string>();
        private readonly List<Button> buttons = new List<Button>();
        private readonly List<UnityEngine.Events.UnityAction> listeners = new List<UnityEngine.Events.UnityAction>();
        private CarliSanctuaryPreview preview;
        private Text title, body, comfort, privacy, observation, state;
        private int currentStep, currentLanguage, currentObservation = -1;
        private int clicks, validationFailures;
        private bool started;
        private string receiptPath;
        private string startedAtUtc;

        private void Start()
        {
            Debug.Log("CARLI_BUILD05_START_INVOKED");
            #if !UNITY_EDITOR
            enabled = false;
            return;
            #else
            startedAtUtc = DateTime.UtcNow.ToString("o");
            receiptPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "CARLi_R4_Evidence", "R5_Build05_PlayModeEvidence.json");
            preview = GetComponentInChildren<CarliSanctuaryPreview>(true);
            if (preview == null) { failures.Add("PREVIEW_CONTROLLER_MISSING"); Save(); return; }
            var texts = GetComponentsInChildren<Text>(true);
            title = FindText(texts, "Moment title");
            body = FindText(texts, "Body");
            comfort = FindText(texts, "Supportive note");
            privacy = FindText(texts, "Privacy boundary");
            observation = FindText(texts, "Observed behavior");
            state = FindText(texts, "PREVIEW MODE");
            if (title == null || body == null || comfort == null || privacy == null || observation == null || state == null)
                failures.Add("DISPLAY_LABEL_MISSING");
            var all = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < Steps.Length; i++) Register(all, Steps[i], 0, i);
            for (int i = 0; i < Languages.Length; i++) Register(all, Languages[i], 1, i);
            for (int i = 0; i < Observations.Length; i++) Register(all, Observations[i], 2, i);
            started = true;
            VerifyLabels();
            Save();
            #endif
        }

        private static Text FindText(Text[] texts, string name)
        {
            foreach (var text in texts) if (text.gameObject.name == name) return text;
            return null;
        }
        private void Register(Button[] all, string name, int kind, int index)
        {
            Button found = null;
            foreach (var b in all) if (b.gameObject.name == name) { found = b; break; }
            if (found == null) { failures.Add("BUTTON_NOT_FOUND:" + name); return; }
            int selectedIndex = index, selectedKind = kind;
            UnityEngine.Events.UnityAction action = () => OnActualClick(selectedKind, selectedIndex, name);
            found.onClick.AddListener(action);
            buttons.Add(found);
            listeners.Add(action);
        }
        private void OnActualClick(int kind, int index, string name)
        {
            // UnityEvent persistent callbacks run before these non-persistent listeners.
            clicks++;
            if (kind == 0) { seenSteps[index] = true; currentStep = index; currentObservation = -1; }
            else if (kind == 1) { seenLanguages[index] = true; currentLanguage = index; }
            else { seenObservations[index] = true; currentObservation = index; }
            actions.Add(DateTime.UtcNow.ToString("o") + " " + name);
            VerifyLabels();
            Save();
        }
        private void VerifyLabels()
        {
            if (title == null || body == null || comfort == null || privacy == null || observation == null || state == null)
                return;
            var language = (CareLanguage)currentLanguage;
            var expected = CareMomentCopy.ForStep(currentStep, language);
            bool okay = title.text == expected.title && body.text == expected.body &&
                comfort.text == expected.comfort && privacy.text == expected.privacy &&
                observation.text == (currentObservation < 0 ? CareMomentCopy.SelectionPrompt(language) :
                    CareMomentCopy.ObservationFact(currentObservation, language)) &&
                state.text.Contains("SCENE PREVIEW ONLY") && state.text.Contains("No consent") &&
                state.text.Contains("No export") && state.text.Contains("No XR tracking");
            if (!okay) { validationFailures++; failures.Add("DISPLAY_MISMATCH_AFTER_CLICK_" + clicks); }
        }
        private static int Count(bool[] array) { int n=0; foreach (bool b in array) if (b) n++; return n; }
        private void Save()
        {
            #if UNITY_EDITOR
            if (string.IsNullOrEmpty(receiptPath)) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(receiptPath));
                bool disclaimer = state != null && state.text.Contains("SCENE PREVIEW ONLY") &&
                    state.text.Contains("No consent") && state.text.Contains("No export") && state.text.Contains("No XR tracking");
                var receipt = new Receipt {
                    startedAtUtc = startedAtUtc, updatedAtUtc = DateTime.UtcNow.ToString("o"),
                    actualButtonClicks = clicks, uniqueSteps = Count(seenSteps),
                    uniqueLanguages = Count(seenLanguages), uniqueObservations = Count(seenObservations),
                    labelValidationFailures = validationFailures, previewDisclaimerVisible = disclaimer,
                    actions = actions.ToArray(), failures = failures.ToArray(),
                    status = started && clicks >= 13 && Count(seenSteps)==6 && Count(seenLanguages)==3 &&
                        Count(seenObservations)==4 && validationFailures==0 && failures.Count==0 && disclaimer
                        ? "PASS_EDITOR_PLAYMODE_UI_ONLY" : "NO_GO_INCOMPLETE_OR_FAILED"
                };
                File.WriteAllText(receiptPath, JsonUtility.ToJson(receipt, true), new System.Text.UTF8Encoding(false));
            }
            catch (Exception e) { Debug.LogError("CARLI_BUILD05_EVIDENCE_WRITE_FAILED: " + e.Message); }
            #endif
        }
        private void OnDisable()
        {
            for (int i = 0; i < buttons.Count; i++) if (buttons[i] != null) buttons[i].onClick.RemoveListener(listeners[i]);
            Save();
        }
    }
}
