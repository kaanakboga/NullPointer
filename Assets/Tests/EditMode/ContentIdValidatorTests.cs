using System.Collections.Generic;
using System.Linq;
using NullPointer.Content;
using NUnit.Framework;

namespace NullPointer.Tests.EditMode
{
    public sealed class ContentIdValidatorTests
    {
        [TestCase("evidence.mert.photo")]
        [TestCase("deduction.mert.postmortem_terminal")]
        [TestCase("location.sector_7.mert_apartment")]
        public void IsValid_WithCanonicalId_ReturnsTrue(string stableId)
        {
            Assert.That(ContentIdValidator.IsValid(stableId), Is.True);
        }

        [TestCase("")]
        [TestCase("Evidence.Mert.Photo")]
        [TestCase("evidence..photo")]
        [TestCase("evidence.mert.ölüm")]
        [TestCase("evidence")]
        public void IsValid_WithMissingOrMalformedId_ReturnsFalse(string stableId)
        {
            Assert.That(ContentIdValidator.IsValid(stableId), Is.False);
        }

        [Test]
        public void Validate_WithDuplicateIds_IdentifiesEveryConflictingAsset()
        {
            var assets = new IStableContent[]
            {
                new FakeContent("evidence.mert.photo", "EV_MertPhotograph"),
                new FakeContent("evidence.mert.photo", "EV_MertPhotographCopy")
            };

            ContentIdDiagnostic duplicate = ContentIdValidator.Validate(assets)
                .Single(item => item.Code == ContentIdDiagnosticCode.DuplicateId);

            Assert.That(duplicate.AssetNames, Is.EquivalentTo(new[]
            {
                "EV_MertPhotograph",
                "EV_MertPhotographCopy"
            }));
            StringAssert.Contains("evidence.mert.photo", duplicate.Message);
        }

        [Test]
        public void Validate_WithBlankId_ReportsOwningAsset()
        {
            IReadOnlyList<ContentIdDiagnostic> diagnostics = ContentIdValidator.Validate(new[]
            {
                new FakeContent(" ", "EV_Unconfigured")
            });

            Assert.That(diagnostics, Has.Count.EqualTo(1));
            Assert.That(diagnostics[0].Code, Is.EqualTo(ContentIdDiagnosticCode.MissingId));
            StringAssert.Contains("EV_Unconfigured", diagnostics[0].Message);
        }

        private sealed class FakeContent : IStableContent
        {
            public FakeContent(string stableId, string diagnosticName)
            {
                StableId = stableId;
                DiagnosticName = diagnosticName;
            }

            public string StableId { get; }

            public string DiagnosticName { get; }
        }
    }
}
