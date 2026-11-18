using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

namespace CarliSenseXR.UnityAdapter
{
    public enum CarliTrackedHand { Left, Right }

    // Executes before CarliXriHoldButton.Update(), which checks same-frame freshness.
    [DefaultExecutionOrder(-200)]
    public sealed class CarliXRHandsTrackingBridge : MonoBehaviour
    {
        [SerializeField] private CarliTrackedHand trackedHand = CarliTrackedHand.Right;
        [Tooltip("Hold buttons controlled by the interactor of this exact hand.")]
        [SerializeField] private CarliXriHoldButton[] targets;

        private readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        private XRHandSubsystem activeSubsystem;
        private int lastDynamicUpdateFrame = -1;
        private bool lastTracked;

        private void OnEnable() { FindSubsystem(); }
        private void OnDisable()
        {
            Unsubscribe();
            Publish(false);
        }
        private void FindSubsystem()
        {
            Unsubscribe();
            subsystems.Clear();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var item in subsystems)
            {
                if (item != null && item.running)
                {
                    activeSubsystem = item;
                    activeSubsystem.updatedHands += OnUpdatedHands;
                    break;
                }
            }
            lastDynamicUpdateFrame = -1;
            lastTracked = false;
        }
        private void Unsubscribe()
        {
            if (activeSubsystem != null) activeSubsystem.updatedHands -= OnUpdatedHands;
            activeSubsystem = null;
        }
        private void OnUpdatedHands(XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags flags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (updateType != XRHandSubsystem.UpdateType.Dynamic ||
                !ReferenceEquals(subsystem, activeSubsystem)) return;
            // No assumption about partial hand-joint updates; reject if the hand isn't tracked.
            lastDynamicUpdateFrame = Time.frameCount;
            lastTracked = trackedHand == CarliTrackedHand.Left
                ? subsystem.leftHand.isTracked : subsystem.rightHand.isTracked;
        }
        private void Update()
        {
            if (activeSubsystem == null || !activeSubsystem.running)
            {
                Publish(false);
                FindSubsystem();
                return;
            }
            // Requiring a Dynamic callback in this exact frame makes stalled updates fail closed.
            bool valid = lastTracked && lastDynamicUpdateFrame == Time.frameCount;
            Publish(valid);
        }
        private void Publish(bool valid)
        {
            if (targets == null) return;
            foreach (var target in targets)
                if (target != null && target.isActiveAndEnabled) target.SetTrackingValid(valid);
        }
    }
}
