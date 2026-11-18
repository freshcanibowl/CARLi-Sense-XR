using UnityEngine;
using UnityEngine.UI;
using CarliSenseXR.UnityAdapter;

namespace CarliSenseXR.CareMoments
{
    public enum CareLanguage { English, Chinese, Malay }

    // Optional UI layer. Does not grant consent, export files, or diagnose an animal.
    // The original R4 Core and R5 XR Hands bridge remain unchanged.
    public sealed class CarliCareMomentsPanel : MonoBehaviour
    {
        [SerializeField] private CarliWorkflowPresenter presenter;
        [SerializeField] private CareLanguage language = CareLanguage.Chinese;
        [Header("UI Text references (Unity UI Text)")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text comfortText;
        [SerializeField] private Text privacyText;
        [SerializeField] private Text primaryActionText;
        [SerializeField] private Text selectedObservationText;

        private int currentStep;
        private int selectedObservation = -1;

        private void OnEnable()
        {
            if (presenter != null && presenter.onStepChanged != null)
                presenter.onStepChanged.AddListener(ShowStep);
            Render();
        }

        private void OnDisable()
        {
            if (presenter != null && presenter.onStepChanged != null)
                presenter.onStepChanged.RemoveListener(ShowStep);
        }

        public void SetLanguage(int index)
        {
            language = (CareLanguage)Mathf.Clamp(index, 0, 2);
            Render();
        }

        // These are neutral caregiver-selected observations, never interpretations or diagnoses.
        public void ChooseObservation(int index)
        {
            if (currentStep != 0 || index < 0 || index > 3) return;
            selectedObservation = index;
            Render();
        }

        // Call only from an explicitly labelled "Use this observation" UI button.
        public void ConfirmSelectedObservation()
        {
            if (presenter == null || currentStep != 0 || selectedObservation < 0) return;
            presenter.SetObservation(CareMomentCopy.ObservationFact(selectedObservation, language));
        }

        // Bind to the workflow's onDraftDiscarded event as well as to a local reset button.
        public void ClearTransientSelection()
        {
            selectedObservation = -1;
            currentStep = 0;
            Render();
        }

        public void ShowStep(int step)
        {
            currentStep = Mathf.Clamp(step, 0, 5);
            if (currentStep != 0) selectedObservation = -1;
            Render();
        }

        private void Render()
        {
            CareMomentCopy.StepContent content = CareMomentCopy.ForStep(currentStep, language);
            Assign(titleText, content.title);
            Assign(bodyText, content.body);
            Assign(comfortText, content.comfort);
            Assign(privacyText, content.privacy);
            Assign(primaryActionText, content.action);
            Assign(selectedObservationText, selectedObservation < 0
                ? CareMomentCopy.SelectionPrompt(language)
                : CareMomentCopy.ObservationFact(selectedObservation, language));
        }

        private static void Assign(Text target, string value)
        {
            if (target != null) target.text = value;
        }
    }
}
