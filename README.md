# CARLi Sense XR

**Care Sanctuary** — a privacy-first companion-care VR experience built for Meta Quest
(Unity 6000.6 / URP / Unity XRI 3.6 + XR Hands 1.9 / Meta XR Core SDK 85).

CARLi guides a caregiver through a six-step **Care Moment** flow —
`01 Observe → 02 Context → 03 Respond → 04 Consent → 05 Journal → 06 Finish` —
in English, 中文, and Bahasa Melayu. Every stage is "SCENE PREVIEW ONLY":
no consent data, no export, no XR tracking of the person being cared for.

## What's in this repository

| Path | Purpose |
| --- | --- |
| `Assets/CARLiR4_Validation/Core/` | Workflow state machine (`CarliWorkflowCore`) and the pinch-gated hold button model (`CarliHoldGate`) |
| `Assets/CARLiR4_Validation/CareMoments/` | Trilingual copy (`CareMomentCopy`) and the Care Moments panel |
| `Assets/CARLiR4_Validation/UnityAdapter/` | XRI hold button, XR Hands tracking bridge, and the eye-gaze highlight (`CarliEyeGazeHighlight`, OVREyeGaze with head-gaze fallback) |
| `Assets/CARLiR4_Validation/Sanctuary/` | The Care Sanctuary scene, preview controller, copy tests |
| `Assets/CARLiR4_Validation/PlayModeEvidence/` | `CarliSanctuaryPlayModeEvidence` — records real Editor Play Mode UI clicks and writes a JSON receipt |
| `Assets/Editor/` | Batch verification scripts (`B053*`, `B054*`) used for mechanical build gates |
| `CARLi_R4_Evidence/` | Evidence receipts (test XML, Play Mode JSON) and the Meta XR SDK Unity 6000.6 compatibility patch bundle (`SDKPatch/`) |

## Verification workflow

The build is gated on mechanical evidence, not "it looks right":

1. **Scene persistence** — the recorder component is written into
   `CARLi_CareSanctuary_B053_Verified.unity` and its script GUID verified on disk.
2. **Play Mode receipt** — entering Play Mode writes
   `CARLi_R4_Evidence/R5_Build05_PlayModeEvidence.json`. A clean receipt has
   `failures: []`, `previewDisclaimerVisible: true`, and `CARLI_BUILD05_START_INVOKED`
   in the Editor log. Full `PASS_EDITOR_PLAYMODE_UI_ONLY` requires 13 real UI clicks
   (6 steps + 3 languages + 4 observations).
3. **Edit Mode tests** — `CARLi_R4_Evidence/R4_22of22_VERIFIED.xml` documents 22/22 passing.

## Meta XR SDK on Unity 6000.6

`com.meta.xr.sdk.core@85.0.0` does not compile against Unity 6000.6.9f1 as published.
`CARLi_R4_Evidence/SDKPatch/` contains the 8 files patched for CS0619-era API changes
(`GetInstanceID` → `GetEntityId`, `Physics.BakeMesh(EntityId,bool)`, V2 drag-drop, etc.)
plus install instructions. Runtime XR features are untouched; only editor-window
conveniences (block drag-drop, hierarchy icons) are disabled.

## Live demo

A web concept demo (UGUI card flow only, no XR runtime) is deployed at
[carli-sense-xr.netlify.app](https://carli-sense-xr.netlify.app/). It previews the
Care Moment interaction flow; the XR rig, hand tracking, and eye-gaze features live
in this Unity project and target Meta Quest hardware.

## Status

- [x] Build 05.3 — Scene Save verified (B053_Verified scene, recorder persisted)
- [x] Play Mode receipt — Start invoked, zero failures
- [x] Meta XR Core SDK installed, OVRCameraRig in scene (Build 05.4)
- [ ] 13-click full interaction pass → `PASS_EDITOR_PLAYMODE_UI_ONLY`
- [ ] Device verification (Quest, XR Hands, eye tracking on Pro-class hardware)
