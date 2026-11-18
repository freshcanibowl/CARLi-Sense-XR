using System;
namespace CarliSenseXR.Core
{
    public enum CareStep { Observe, Context, Respond, Consent, Journal, Finish }
    [Serializable]
    public sealed class CareJournal
    {
        public string schema_version = "carli-journal-v1";
        public bool synthetic_demo = true;
        public string session_id, created_utc;
        public string observation_source = "caregiver_selected";
        public string observation_fact;
        public string context_source = "caregiver_selected";
        public string context_environment;
        public string interpretation = "unknown_not_diagnosed";
        public string chosen_response;
        public bool export_consented;
    }
    public sealed class CarliWorkflowCore
    {
        private CareJournal draft;
        public CareJournal Draft { get { return Snapshot(); } }
        public CareStep Step { get; private set; }
        public bool IsStopped { get; private set; }
        public bool HasExported { get; private set; }
        public string LastError { get; private set; } = "";
        public CarliWorkflowCore() { StartNewSession(); }
        public void StartNewSession()
        {
            draft = new CareJournal { session_id = Guid.NewGuid().ToString("N"), created_utc = DateTime.UtcNow.ToString("o") };
            Step = CareStep.Observe; IsStopped = false; HasExported = false; LastError = "";
        }
        private bool Fail(string error) { LastError = error; return false; }
        private bool Success() { LastError = ""; return true; }
        private bool In(CareStep step) { return !IsStopped && draft != null && Step == step; }
        public bool SetObservation(string value)
        {
            if (!In(CareStep.Observe)) return Fail("wrong_step");
            if (string.IsNullOrWhiteSpace(value)) return Fail("observation_required");
            draft.observation_fact = value.Trim(); return Success();
        }
        public bool SetContext(string value)
        {
            if (!In(CareStep.Context)) return Fail("wrong_step");
            if (string.IsNullOrWhiteSpace(value)) return Fail("context_required");
            draft.context_environment = value.Trim(); return Success();
        }
        public bool SetResponse(string value)
        {
            if (!In(CareStep.Respond)) return Fail("wrong_step");
            if (string.IsNullOrWhiteSpace(value)) return Fail("response_required");
            draft.chosen_response = value.Trim(); return Success();
        }
        private bool Complete() { return draft != null && !string.IsNullOrWhiteSpace(draft.observation_fact) && !string.IsNullOrWhiteSpace(draft.context_environment) && !string.IsNullOrWhiteSpace(draft.chosen_response); }
        public bool Next()
        {
            if (IsStopped) return Fail("stopped");
            switch (Step)
            {
                case CareStep.Observe:
                    if (string.IsNullOrWhiteSpace(draft.observation_fact)) return Fail("observation_required");
                    Step = CareStep.Context; return Success();
                case CareStep.Context:
                    if (string.IsNullOrWhiteSpace(draft.context_environment)) return Fail("context_required");
                    Step = CareStep.Respond; return Success();
                case CareStep.Respond:
                    if (string.IsNullOrWhiteSpace(draft.chosen_response)) return Fail("response_required");
                    Step = CareStep.Consent; return Success();
                case CareStep.Consent: return Fail("explicit_consent_required");
                case CareStep.Journal:
                    if (!HasExported) return Fail("journal_export_required");
                    Step = CareStep.Finish; return Success();
                default: return Fail("already_finished");
            }
        }
        public bool GrantConsent()
        {
            if (!In(CareStep.Consent)) return Fail("wrong_step");
            if (!Complete()) return Fail("incomplete_evidence");
            draft.export_consented = true; Step = CareStep.Journal; return Success();
        }
        public bool DeclineConsent()
        {
            if (!In(CareStep.Consent)) return Fail("wrong_step");
            StopCurrentDraft(); return true;
        }
        public bool CanExport { get { return In(CareStep.Journal) && draft.export_consented && Complete() && !HasExported; } }
        public CareJournal GetAuthorizedExportSnapshot()
        {
            if (!CanExport) { Fail("export_not_authorized"); return null; }
            LastError = ""; return Snapshot();
        }
        public bool MarkExportSucceeded()
        {
            if (!CanExport) return Fail("export_not_authorized");
            HasExported = true; return Success();
        }
        public void StopCurrentDraft()
        {
            draft = null; IsStopped = true; HasExported = false; Step = CareStep.Observe; LastError = "";
        }
        private CareJournal Snapshot()
        {
            if (draft == null) return null;
            return new CareJournal { schema_version = draft.schema_version, synthetic_demo = draft.synthetic_demo,
                session_id = draft.session_id, created_utc = draft.created_utc,
                observation_source = draft.observation_source, observation_fact = draft.observation_fact,
                context_source = draft.context_source, context_environment = draft.context_environment,
                interpretation = draft.interpretation, chosen_response = draft.chosen_response,
                export_consented = draft.export_consented };
        }
    }
}
