using UnityEngine;
using UnityEngine.UI;
using CarliSenseXR.CareMoments;

namespace CarliSenseXR.Sanctuary
{
    // Scene review only. NEVER changes the real R4 workflow, consent, files, or tracking.
    public sealed class CarliSanctuaryPreview : MonoBehaviour
    {
        [SerializeField] private Text titleLabel, bodyLabel, comfortLabel, privacyLabel, observationLabel, stateLabel;
        private CareLanguage language = CareLanguage.English;
        private int step;
        private int observation = -1;

        private void Start() { Render(); }
        public void SetLanguage(int index) { language = (CareLanguage)Mathf.Clamp(index, 0, 2); Render(); }
        public void SetStep(int index) { step = Mathf.Clamp(index, 0, 5); observation = -1; Render(); }
        public void SelectObservation(int index) { observation = index >= 0 && index < 4 ? index : -1; Render(); }

        private void Render()
        {
            var content = CareMomentCopy.ForStep(step, language);
            Put(titleLabel, content.title);
            Put(bodyLabel, content.body);
            Put(comfortLabel, content.comfort);
            Put(privacyLabel, content.privacy);
            Put(observationLabel, observation < 0 ? CareMomentCopy.SelectionPrompt(language)
                : CareMomentCopy.ObservationFact(observation, language));
            Put(stateLabel, "SCENE PREVIEW ONLY  |  No consent  |  No export  |  No XR tracking");
        }
        private static void Put(Text target, string value) { if (target != null) target.text = value; }
    }
}
