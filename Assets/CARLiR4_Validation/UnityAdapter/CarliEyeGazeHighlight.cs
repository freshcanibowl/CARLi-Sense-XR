using UnityEngine;
using UnityEngine.UI;

namespace CarliSenseXR.Sanctuary.Evidence
{
    // Gaze highlight for the staged action cards. Uses OVREyeGaze when the
    // hardware provides eye tracking (Quest Pro class); on devices without it,
    // falls back to the head gaze direction so the highlight still works.
    // Pinch confirmation stays in CarliHoldGate — this component only visualizes
    // the "looked at" state so users never need to look at their controllers.
    public sealed class CarliEyeGazeHighlight : MonoBehaviour
    {
        [SerializeField] private Graphic highlightTarget;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoveredColor = new Color(0.55f, 0.85f, 1f);
        [SerializeField] private float gazeCosThreshold = 0.94f; // ~20 degrees
        [SerializeField] private float gazeRange = 3f;

        private OVREyeGaze _eyeGaze;
        private Camera _centerEye;

        public bool GazeHovered { get; private set; }

        private void Start()
        {
            _eyeGaze = FindAnyObjectByType<OVREyeGaze>();
            _centerEye = Camera.main;
            if (highlightTarget != null) highlightTarget.color = normalColor;
        }

        private void Update()
        {
            Vector3 origin, direction;
            if (_eyeGaze != null && _eyeGaze.EyeTrackingEnabled && _eyeGaze.Confidence > 0.5f)
            {
                origin = _eyeGaze.transform.position;
                direction = _eyeGaze.transform.forward;
            }
            else if (_centerEye != null)
            {
                origin = _centerEye.transform.position;
                direction = _centerEye.transform.forward;
            }
            else
            {
                SetHovered(false);
                return;
            }

            Vector3 toCard = transform.position - origin;
            float distance = toCard.magnitude;
            bool hovered = distance > 0.001f && distance <= gazeRange &&
                Vector3.Dot(toCard / distance, direction) >= gazeCosThreshold;
            SetHovered(hovered);
        }

        private void SetHovered(bool hovered)
        {
            if (GazeHovered == hovered) return;
            GazeHovered = hovered;
            if (highlightTarget != null)
                highlightTarget.color = hovered ? hoveredColor : normalColor;
        }
    }
}
