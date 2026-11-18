using NUnit.Framework;
using CarliSenseXR.CareMoments;

namespace CarliSenseXR.Sanctuary.Tests
{
    [TestFixture]
    public class CarliSanctuaryCopyTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void EveryLanguageHasSixCompleteStages(int lang)
        {
            for (int i = 0; i < 6; ++i)
            {
                var s = CareMomentCopy.ForStep(i, (CareLanguage)lang);
                Assert.IsNotEmpty(s.title);
                Assert.IsNotEmpty(s.body);
                Assert.IsNotEmpty(s.comfort);
                Assert.IsNotEmpty(s.privacy);
                Assert.IsNotEmpty(s.action);
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void EachLanguageHasFourNeutralObservations(int lang)
        {
            for (int i=0; i<4; ++i)
                Assert.IsNotEmpty(CareMomentCopy.ObservationFact(i,(CareLanguage)lang));
            Assert.AreEqual("", CareMomentCopy.ObservationFact(-1,(CareLanguage)lang));
            Assert.AreEqual("", CareMomentCopy.ObservationFact(4,(CareLanguage)lang));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ConsentStepExplicitlyMentionsPermission(int lang)
        {
            var s = CareMomentCopy.ForStep(3,(CareLanguage)lang);
            var combined = (s.body+" "+s.privacy).ToLowerInvariant();
            Assert.IsTrue(combined.Contains("consent") || combined.Contains("同意") || combined.Contains("授权") || combined.Contains("persetujuan"));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void JournalStepDisclosesExportedFiles(int lang)
        {
            var s = CareMomentCopy.ForStep(4,(CareLanguage)lang);
            Assert.IsNotEmpty(s.privacy);
            Assert.IsTrue(s.privacy.ToLowerInvariant().Contains("export") || s.privacy.Contains("导出") || s.privacy.ToLowerInvariant().Contains("eksport"));
        }
        [Test]
        public void ChineseConsentDoesNotPromiseDeleteOfExportedFiles()
        {
            var s=CareMomentCopy.ForStep(4,CareLanguage.Chinese);
            StringAssert.Contains("不会删除",s.privacy);
        }
        [Test]
        public void InvalidStepFallsBackToObserve()
        {
            Assert.AreEqual(CareMomentCopy.ForStep(0,CareLanguage.English).title,
                CareMomentCopy.ForStep(900,CareLanguage.English).title);
        }
    }
}
