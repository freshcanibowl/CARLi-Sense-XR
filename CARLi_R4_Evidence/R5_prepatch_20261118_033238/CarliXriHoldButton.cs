using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using CarliSenseXR.Core;
namespace CarliSenseXR.UnityAdapter
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class CarliXriHoldButton : MonoBehaviour
    {
        [SerializeField] private float holdSeconds = 0.8f;
        [SerializeField] private Image progressRing;
        [SerializeField] private UnityEvent onConfirmed;
        private XRSimpleInteractable interactable;
        private CarliHoldGate gate;
        private IXRSelectInteractor owner;
        private bool hoveredByOwner;
        private bool trackingValid;
        private int lastTrackingUpdateFrame = -1;
        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            gate = new CarliHoldGate(Mathf.Max(0.1f, holdSeconds));
            Render();
        }
        private void OnEnable()
        {
            interactable.hoverEntered.AddListener(HoverStart);
            interactable.hoverExited.AddListener(HoverEnd);
            interactable.selectEntered.AddListener(SelectStart);
            interactable.selectExited.AddListener(SelectEnd);
        }
        private void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(HoverStart);
            interactable.hoverExited.RemoveListener(HoverEnd);
            interactable.selectEntered.RemoveListener(SelectStart);
            interactable.selectExited.RemoveListener(SelectEnd);
            Abort();
        }
        private void OnApplicationPause(bool pause) { if (pause) Abort(); }
        private void OnApplicationFocus(bool focus) { if (!focus) Abort(); }
        // Must be supplied by a *real* XR Hands adapter every Unity frame, prior to Update.
        // Until connected, the consent/stop action fails closed. Hover is not eye gaze.
        public void SetTrackingValid(bool valid)
        {
            trackingValid = valid;
            lastTrackingUpdateFrame = Time.frameCount;
            if (!valid) Abort();
        }
        private void HoverStart(HoverEnterEventArgs args)
        {
            if (owner != null && ReferenceEquals(args.interactorObject, owner)) hoveredByOwner = true;
        }
        private void HoverEnd(HoverExitEventArgs args)
        {
            if (owner != null && ReferenceEquals(args.interactorObject, owner)) Abort();
        }
        private void SelectStart(SelectEnterEventArgs args)
        {
            if (owner != null) return;
            owner = args.interactorObject;
            // XRI hover is a spatial ray interaction, not proof of eye-tracking.
            hoveredByOwner = owner is IXRHoverInteractor hoverer && interactable.IsHovered(hoverer);
            gate.Reset();
        }
        private void SelectEnd(SelectExitEventArgs args)
        {
            if (owner != null && ReferenceEquals(args.interactorObject, owner)) Abort();
        }
        private void Update()
        {
            bool fresh = trackingValid && lastTrackingUpdateFrame == Time.frameCount;
            if (gate.Tick(Time.unscaledDeltaTime, owner != null && hoveredByOwner, owner != null, fresh))
                onConfirmed?.Invoke();
            Render();
        }
        private void Abort() { owner = null; hoveredByOwner = false; trackingValid = false; lastTrackingUpdateFrame = -1; if (gate != null) gate.Reset(); Render(); }
        private void Render() { if (gate != null && progressRing != null) progressRing.fillAmount = gate.Progress; }
    }
}
