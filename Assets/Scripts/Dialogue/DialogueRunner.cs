using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;

namespace NullPointer.Dialogue
{
    public sealed class DialogueRunner
    {
        private readonly GameState _gameState;
        private readonly Dictionary<string, DialogueNode> _nodes =
            new Dictionary<string, DialogueNode>(StringComparer.Ordinal);
        private readonly List<DialogueChoice> _availableChoices = new List<DialogueChoice>();

        public DialogueRunner(GameState gameState)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public event Action<DialogueAction> ActionRequested;

        public event Action Completed;

        public DialogueNode CurrentNode { get; private set; }

        public IReadOnlyList<DialogueChoice> AvailableChoices => _availableChoices;

        public bool IsRunning => CurrentNode != null;

        public string LastError { get; private set; } = string.Empty;

        public bool Start(DialogueData dialogue)
        {
            Reset();
            if (dialogue == null)
            {
                LastError = "Dialogue data is null.";
                return false;
            }

            foreach (DialogueNode node in dialogue.Nodes.Where(node => node != null))
            {
                if (string.IsNullOrWhiteSpace(node.NodeId) || !_nodes.TryAdd(node.NodeId, node))
                {
                    LastError = $"Dialogue '{dialogue.StableId}' contains a blank or duplicate node ID.";
                    ResetRuntimeState();
                    return false;
                }
            }

            string startNodeId = string.IsNullOrWhiteSpace(dialogue.StartNodeId)
                ? dialogue.Nodes.FirstOrDefault(node => node != null)?.NodeId
                : dialogue.StartNodeId;

            return EnterNode(startNodeId);
        }

        public bool Advance()
        {
            if (CurrentNode == null || _availableChoices.Count > 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentNode.NextNodeId))
            {
                Complete();
                return true;
            }

            return EnterNode(CurrentNode.NextNodeId);
        }

        public bool SelectChoice(int availableChoiceIndex)
        {
            if (CurrentNode == null ||
                availableChoiceIndex < 0 ||
                availableChoiceIndex >= _availableChoices.Count)
            {
                return false;
            }

            DialogueChoice choice = _availableChoices[availableChoiceIndex];
            RequestActions(choice.Actions);

            if (string.IsNullOrWhiteSpace(choice.NextNodeId))
            {
                Complete();
                return true;
            }

            return EnterNode(choice.NextNodeId);
        }

        public void Stop()
        {
            ResetRuntimeState();
        }

        private bool EnterNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || !_nodes.TryGetValue(nodeId, out DialogueNode node))
            {
                LastError = $"Dialogue node '{nodeId}' could not be resolved; the conversation ended safely.";
                Complete();
                return false;
            }

            CurrentNode = node;
            _availableChoices.Clear();
            foreach (DialogueChoice choice in node.Choices)
            {
                if (choice != null && IsAvailable(choice))
                {
                    _availableChoices.Add(choice);
                }
            }

            RequestActions(node.Actions);
            return true;
        }

        private bool IsAvailable(DialogueChoice choice)
        {
            bool hasFlag = string.IsNullOrWhiteSpace(choice.RequiredStoryFlag) ||
                           _gameState.HasStoryFlag(choice.RequiredStoryFlag);
            bool hasEvidence = string.IsNullOrWhiteSpace(choice.RequiredEvidenceId) ||
                               _gameState.HasCollectedEvidence(choice.RequiredEvidenceId);
            return hasFlag && hasEvidence;
        }

        private void RequestActions(IReadOnlyList<DialogueAction> actions)
        {
            foreach (DialogueAction action in actions)
            {
                if (action != null)
                {
                    ActionRequested?.Invoke(action);
                }
            }
        }

        private void Complete()
        {
            ResetRuntimeState();
            Completed?.Invoke();
        }

        private void Reset()
        {
            _nodes.Clear();
            LastError = string.Empty;
            ResetRuntimeState();
        }

        private void ResetRuntimeState()
        {
            CurrentNode = null;
            _availableChoices.Clear();
        }
    }
}
