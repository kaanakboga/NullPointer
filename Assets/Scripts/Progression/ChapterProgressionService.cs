using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;

namespace NullPointer.Progression
{
    public sealed class ChapterProgressionService
    {
        private readonly GameState _gameState;

        public ChapterProgressionService(GameState gameState)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public bool TryComplete(
            string chapterId,
            IEnumerable<string> requiredDeductionIds,
            string completionFlag)
        {
            string[] requirements = (requiredDeductionIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (requirements.Any(id => !_gameState.HasCompletedDeduction(id)))
            {
                return false;
            }

            bool completed = _gameState.CompleteChapter(chapterId);
            if (!string.IsNullOrWhiteSpace(completionFlag))
            {
                _gameState.SetStoryFlag(completionFlag);
            }

            return completed;
        }
    }
}
