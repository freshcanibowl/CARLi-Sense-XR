# CARLi Sense XR — Submission Readiness (6-ship)

Baseline: `freshcanibowl/CARLi-Sense-XR` `main` @ `201bfa9`. Date: 2026-11-18.

## What was built

Privacy-first companion-care VR proof of concept for Meta Quest
(Unity 6000.6.9f1 / URP / XRI 3.6.1 / XR Hands 1.9 / Meta XR Core SDK 85).
The Care Sanctuary scene walks a caregiver through a six-step Care Moment —
`01 Observe → 02 Context → 03 Respond → 04 Consent → 05 Journal → 06 Finish` —
in English, 中文, and Bahasa Melayu, with a persistent "SCENE PREVIEW ONLY"
disclaimer (no consent capture, no export, no XR tracking of the care recipient).

## Evidence on file (all verified, not claimed)

| Gate | Evidence | Location |
| --- | --- | --- |
| Edit Mode tests | 22/22 pass | `CARLi_R4_Evidence/R4_22of22_VERIFIED.xml` |
| Scene persistence | recorder GUID in scene = 1 | `CARLi_CareSanctuary_B053_Verified.unity` |
| Play Mode Start | `CARLI_BUILD05_START_INVOKED` in editor log | `B055_run4.log` (local) |
| 13-click interaction | `PASS_EDITOR_PLAYMODE_UI_ONLY`, 13 clicks, 6/6 steps, 3/3 languages, 4/4 observations, 0 failures | `CARLi_R4_Evidence/R5_Build05_PlayModeEvidence.json` |
| Meta SDK on Unity 6000.6 | compiles clean after 8-file patch | `CARLi_R4_Evidence/SDKPatch/` |

## Shipped assets

- **Public repo**: https://github.com/freshcanibowl/CARLi-Sense-XR (PUBLIC, no secrets in history — checked before publishing; `cloudProjectId` change deliberately excluded)
- **Live web concept demo**: https://carli-sense-xr.netlify.app (Three.js preview of the Care Moment flow; no XR runtime)
- **README** documents verification workflow, SDK patch, and honest status

## Remaining before Devpost submit

| Item | Owner | Notes |
| --- | --- | --- |
| Demo video (< per form's limit) | Learner | Record: open Unity → B053_Verified scene → Play → click through the 13 Care Moment interactions; the PASS receipt appearing in `CARLi_R4_Evidence/` is the proof moment. The Netlify demo can be shown second. No credentials/personal windows on screen. |
| Video upload (public YouTube or form-approved host) | Learner | Verify link opens logged-out. |
| Project name + short description + form answers | Learner | Must be written by the learner; agent may fix spelling/grammar only. |
| Exit survey | Learner | Own reflections. |
| LICENSE file in repo | Agent on request | No license file currently; decide MIT or other before adding — adding one is an authorization decision. |
| Optional: device pass (Quest / XR Hands / eye gaze) | Learner | Not a submission requirement; current status is honestly documented as NOT VERIFIED. |

## Reproduce the evidence locally

```powershell
$project = "C:\Users\Asus\Documents\CARLi Sense XR\CARLi Sense XR"
# 1. Open in Unity 6000.6.9f1, load Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity
# 2. Enter Play Mode; confirm Console: CARLI_BUILD05_START_INVOKED
# 3. Click all 13 buttons (6 steps, 3 languages, 4 observations)
# 4. Check CARLi_R4_Evidence/R5_Build05_PlayModeEvidence.json → PASS_EDITOR_PLAYMODE_UI_ONLY
# Batch-mode equivalent:
# Unity.exe -batchmode -nographics -projectPath <path> -executeMethod B055FullInteraction.Run
```
