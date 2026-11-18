# Demo Video Recording Checklist (checklist for shooting, one pass ≈ 90 seconds)

## Before recording
- [ ] Close unrelated windows (browsers, chat, other projects)
- [ ] Unity is open, Edit Mode, no console errors
- [ ] `CARLi_R4_Evidence/R5_Build05_PlayModeEvidence.json` timestamp is today
      (it was refreshed 2026-11-18 13:40 UTC via batch run)

## Shot list (in order)
- [ ] 1. Load scene: `Assets/CARLiR4_Validation/Sanctuary/Scenes/CARLi_CareSanctuary_B053_Verified.unity`
- [ ] 2. Enter Play Mode → point at Console line `CARLI_BUILD05_START_INVOKED`
- [ ] 3. Click the six step buttons in order: 01 Observe → 02 Context → 03 Respond → 04 Consent → 05 Journal → 06 Finish
- [ ] 4. Click the three language buttons: English → 中文 → BM (show 中文 copy on screen)
- [ ] 5. Click the four observation buttons: Observation 1 → 4
- [ ] 6. Open `CARLi_R4_Evidence/R5_Build05_PlayModeEvidence.json` in an editor —
       frame `PASS_EDITOR_PLAYMODE_UI_ONLY`, `"actualButtonClicks": 13`, `"failures": []`
- [ ] 7. (Optional closer) Browser: GitHub repo + Netlify demo

## After recording
- [ ] Watch it once: is the PASS receipt readable on screen?
- [ ] Upload to public YouTube (or form-approved host), verify link opens logged-out
- [ ] Put the URL into the Devpost submission form
