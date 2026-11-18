using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using CarliSenseXR.Core;
namespace CarliSenseXR.UnityAdapter
{
    [Serializable] public sealed class StepChangedEvent : UnityEvent<int> { }
    public sealed class CarliWorkflowPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject[] stepPanels = new GameObject[6];
        private const string JournalSubfolder = "CarliJournals";
        public StepChangedEvent onStepChanged;
        public UnityEvent onDraftDiscarded;
        public UnityEvent<string> onExportSuccess;
        public UnityEvent<string> onError;
        private CarliWorkflowCore workflow;
        private void Awake() { workflow = new CarliWorkflowCore(); Refresh(); }
        private void Apply(Func<bool> action) { if (!action()) onError?.Invoke(workflow.LastError); Refresh(); }
        public void SetObservation(string value) { Apply(() => workflow.SetObservation(value)); }
        public void SetContext(string value) { Apply(() => workflow.SetContext(value)); }
        public void SetResponse(string value) { Apply(() => workflow.SetResponse(value)); }
        public void Next() { Apply(() => workflow.Next()); }
        public void GrantConsentAfterConfirmedHold() { Apply(() => workflow.GrantConsent()); }
        public void DeclineConsent() { Apply(() => workflow.DeclineConsent()); if (workflow.IsStopped) onDraftDiscarded?.Invoke(); }
        public void StopDiscardCurrentDraft() { workflow.StopCurrentDraft(); Refresh(); onDraftDiscarded?.Invoke(); }
        public void NewSession() { workflow.StartNewSession(); Refresh(); }
        public void ExportJournal()
        {
            var snapshot = workflow.GetAuthorizedExportSnapshot();
            if (snapshot == null) { onError?.Invoke(workflow.LastError); return; }
            string tempPath = null;
            try
            {
                var dir = Path.Combine(Application.persistentDataPath, JournalSubfolder);
                Directory.CreateDirectory(dir);
                var name = "CARLi_" + Guid.NewGuid().ToString("N") + ".json";
                var finalPath = Path.Combine(dir, name);
                tempPath = Path.Combine(dir, "." + name + ".pending");
                var json = JsonUtility.ToJson(snapshot, true);
                using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false))) sw.Write(json);
                if (!workflow.CanExport) { onError?.Invoke("export_authorization_changed"); return; }
                File.Move(tempPath, finalPath);
                tempPath = null;
                if (!workflow.MarkExportSucceeded())
                {
                    try { File.Delete(finalPath); } catch (Exception) { onError?.Invoke("orphaned_export_file_requires_review"); }
                    onError?.Invoke("export_commit_failed"); return;
                }
                onExportSuccess?.Invoke(finalPath);
                Refresh();
            }
            catch (Exception e) { onError?.Invoke("journal_write_failed:" + e.GetType().Name); }
            finally
            {
                if (tempPath != null) try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch (Exception) { onError?.Invoke("temp_cleanup_failed"); }
            }
        }
        private void Refresh()
        {
            if (workflow == null) return;
            for (int i = 0; i < stepPanels.Length; i++)
                if (stepPanels[i] != null) stepPanels[i].SetActive(!workflow.IsStopped && i == (int)workflow.Step);
            onStepChanged?.Invoke((int)workflow.Step);
        }
    }
}
