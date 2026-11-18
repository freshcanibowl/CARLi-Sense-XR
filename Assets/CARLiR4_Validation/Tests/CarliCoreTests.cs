using System;
using NUnit.Framework;
using CarliSenseXR.Core;
public class CarliCoreTests
{
    private CarliWorkflowCore ReadyForConsent()
    {
        var w = new CarliWorkflowCore();
        Assert.True(w.SetObservation("Dog paused near doorway"));
        Assert.True(w.Next());
        Assert.True(w.SetContext("Doorway, daytime; cause unknown"));
        Assert.True(w.Next());
        Assert.True(w.SetResponse("Observe and monitor; no diagnosis"));
        Assert.True(w.Next());
        Assert.AreEqual(CareStep.Consent, w.Step);
        return w;
    }
    [Test] public void CannotSkipEmptyObservation() { var w = new CarliWorkflowCore(); Assert.False(w.Next()); Assert.AreEqual(CareStep.Observe,w.Step); }
    [Test] public void CannotGrantEarly() { var w = new CarliWorkflowCore(); Assert.False(w.GrantConsent()); Assert.False(w.CanExport); }
    [Test] public void CannotAdvanceConsentByNext() { var w = ReadyForConsent(); Assert.False(w.Next()); Assert.AreEqual(CareStep.Consent,w.Step); }
    [Test] public void DeclineDiscardsDraft() { var w = ReadyForConsent(); Assert.True(w.DeclineConsent()); Assert.True(w.IsStopped); Assert.IsNull(w.Draft); Assert.False(w.CanExport); }
    [Test] public void AuthorizedExportAndFinish() { var w = ReadyForConsent(); Assert.True(w.GrantConsent()); Assert.True(w.CanExport); Assert.False(w.Next()); Assert.True(w.MarkExportSucceeded()); Assert.True(w.Next()); Assert.AreEqual(CareStep.Finish,w.Step); }
    [Test] public void StopClearsCurrentDraft() { var w = ReadyForConsent(); w.GrantConsent(); w.StopCurrentDraft(); Assert.False(w.CanExport); Assert.IsNull(w.Draft); Assert.False(w.Next()); }
    [Test] public void DraftSnapshotCannotChangeInternalState() { var w = new CarliWorkflowCore(); var x = w.Draft; x.observation_fact = "injected"; x.export_consented = true; Assert.False(w.Next()); Assert.False(w.CanExport); Assert.IsNull(w.Draft.observation_fact); }
    [Test] public void ExportSnapshotBlockedBeforeConsent() { var w = ReadyForConsent(); Assert.IsNull(w.GetAuthorizedExportSnapshot()); Assert.AreEqual("export_not_authorized", w.LastError); }
    [Test] public void AuthorizedSnapshotIsDefensive() { var w = ReadyForConsent(); Assert.True(w.GrantConsent()); var a = w.GetAuthorizedExportSnapshot(); Assert.IsNotNull(a); a.export_consented = false; a.observation_fact = "tampered"; var b = w.GetAuthorizedExportSnapshot(); Assert.True(b.export_consented); Assert.AreEqual("Dog paused near doorway", b.observation_fact); }
    [Test] public void ExportCannotBeRepeated() { var w = ReadyForConsent(); Assert.True(w.GrantConsent()); Assert.True(w.MarkExportSucceeded()); Assert.False(w.CanExport); Assert.IsNull(w.GetAuthorizedExportSnapshot()); Assert.False(w.MarkExportSucceeded()); }
    [Test] public void StopInvalidatesPreviouslyCapturedSnapshotAuthorization() { var w = ReadyForConsent(); Assert.True(w.GrantConsent()); var oldSnapshot = w.GetAuthorizedExportSnapshot(); w.StopCurrentDraft(); Assert.IsNotNull(oldSnapshot); Assert.False(w.CanExport); Assert.IsNull(w.GetAuthorizedExportSnapshot()); }
    [Test] public void HoldCancelsOnRelease() { var h = new CarliHoldGate(.8f); for (int i=0;i<3;i++) h.Tick(.1f,true,true,true); h.Tick(.01f,true,false,true); Assert.AreEqual(0f,h.Progress); Assert.False(h.Confirmed); }
    [Test] public void HoldRejectsTrackingLoss() { var h = new CarliHoldGate(.8f); for (int i=0;i<5;i++)h.Tick(.1f,true,true,true); h.Tick(.01f,true,true,false); Assert.AreEqual(0f,h.Progress); }
    [Test] public void HoldConfirmsOnce() { var h = new CarliHoldGate(1f); int fired=0; for(int i=0;i<11;i++)if(h.Tick(.1f,true,true,true)) fired++; Assert.AreEqual(1,fired); }
    [Test] public void HoldFailsClosedOnLongStall() { var h = new CarliHoldGate(.8f); h.Tick(.1f,true,true,true); Assert.False(h.Tick(1f,true,true,true)); Assert.AreEqual(0f,h.Progress); }
    [Test] public void HoldRejectsNaNFrameTime() { var h = new CarliHoldGate(.8f); h.Tick(.1f,true,true,true); Assert.False(h.Tick(float.NaN,true,true,true)); Assert.AreEqual(0f,h.Progress); }
    [Test] public void HoldRejectsInfiniteFrameTime() { var h = new CarliHoldGate(.8f); Assert.False(h.Tick(float.PositiveInfinity,true,true,true)); Assert.AreEqual(0f,h.Progress); }
    [Test] public void HoldRejectsInvalidThreshold() { Assert.Throws<ArgumentOutOfRangeException>(() => new CarliHoldGate(float.NaN)); Assert.Throws<ArgumentOutOfRangeException>(() => new CarliHoldGate(float.PositiveInfinity)); Assert.Throws<ArgumentOutOfRangeException>(() => new CarliHoldGate(0f)); }
    [Test] public void StopAfterExportDoesNotPretendToDeleteSavedFile() { var w=ReadyForConsent(); Assert.True(w.GrantConsent()); Assert.True(w.MarkExportSucceeded()); w.StopCurrentDraft(); Assert.IsNull(w.Draft); Assert.False(w.CanExport); }
    [Test] public void HoldRejectsZeroAndNegativeFrameTime() { var h=new CarliHoldGate(.8f); h.Tick(.1f,true,true,true); Assert.False(h.Tick(0f,true,true,true)); Assert.AreEqual(0f,h.Progress); h.Tick(.1f,true,true,true); Assert.False(h.Tick(-.1f,true,true,true)); Assert.AreEqual(0f,h.Progress); }
    [Test] public void StopRequiresExplicitNewSession() { var w=ReadyForConsent(); w.StopCurrentDraft(); Assert.False(w.SetObservation("after-stop")); Assert.False(w.GrantConsent()); Assert.False(w.CanExport); w.StartNewSession(); Assert.AreEqual(CareStep.Observe,w.Step); Assert.False(w.IsStopped); Assert.False(w.Next()); }
    [Test] public void ExportFailureDoesNotClaimFinish() { var w=ReadyForConsent(); Assert.True(w.GrantConsent()); Assert.False(w.Next()); Assert.AreEqual(CareStep.Journal,w.Step); Assert.False(w.HasExported); }
}
