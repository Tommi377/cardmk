using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RealMK;

/// <summary>
/// Card control panel for manual draw/play/end-turn testing.
/// </summary>
public partial class CardPlayPanel : PanelContainer
{
    private readonly PlayerId _playerId = new(0);
    private readonly List<string> _logLines = new();

    private GameManager? _gameManager;
    private bool _subscribed;
    private CardInstanceId? _selectedCardId;

    /// <summary>
    /// Node path to the active <see cref="GameManager"/> instance.
    /// </summary>
    [Export]
    public NodePath GameManagerPath { get; set; } = new NodePath("../SubViewportContainer/SubViewport/GameManager");

    private Button _startRoundButton = null!;
    private SpinBox _drawCountSpinBox = null!;
    private Button _drawButton = null!;
    private Button _endTurnButton = null!;
    private Label _phaseLabel = null!;
    private Label _deckSummaryLabel = null!;
    private Label _resourcesLabel = null!;
    private ItemList _handList = null!;
    private OptionButton _playModeOption = null!;
    private SpinBox _choiceIndexSpinBox = null!;
    private OptionButton _sidewaysResourceOption = null!;
    private Button _playButton = null!;
    private RichTextLabel _eventLog = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        base._Ready();
        CacheNodes();
        ConfigureControls();
        WireSignals();
        TryResolveGameManager();
        RefreshUi();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_gameManager == null || !_subscribed)
        {
            TryResolveGameManager();
        }
    }

    private void CacheNodes()
    {
        _startRoundButton = GetNode<Button>("MarginContainer/VBoxContainer/TopActionsRow/StartRoundButton");
        _drawCountSpinBox = GetNode<SpinBox>("MarginContainer/VBoxContainer/TopActionsRow/DrawCountSpinBox");
        _drawButton = GetNode<Button>("MarginContainer/VBoxContainer/TopActionsRow/DrawButton");
        _endTurnButton = GetNode<Button>("MarginContainer/VBoxContainer/TopActionsRow/EndTurnButton");
        _phaseLabel = GetNode<Label>("MarginContainer/VBoxContainer/PhaseLabel");
        _deckSummaryLabel = GetNode<Label>("MarginContainer/VBoxContainer/DeckSummaryLabel");
        _resourcesLabel = GetNode<Label>("MarginContainer/VBoxContainer/ResourcesLabel");
        _handList = GetNode<ItemList>("MarginContainer/VBoxContainer/HandList");
        _playModeOption = GetNode<OptionButton>("MarginContainer/VBoxContainer/PlayOptionsRow/PlayModeOption");
        _choiceIndexSpinBox = GetNode<SpinBox>("MarginContainer/VBoxContainer/PlayOptionsRow/ChoiceIndexSpinBox");
        _sidewaysResourceOption = GetNode<OptionButton>("MarginContainer/VBoxContainer/PlayOptionsRow/SidewaysResourceOption");
        _playButton = GetNode<Button>("MarginContainer/VBoxContainer/PlayButton");
        _eventLog = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/EventLog");
    }

    private void ConfigureControls()
    {
        _drawCountSpinBox.MinValue = 1;
        _drawCountSpinBox.Step = 1;
        _drawCountSpinBox.Value = 1;

        _choiceIndexSpinBox.MinValue = 0;
        _choiceIndexSpinBox.Step = 1;
        _choiceIndexSpinBox.Value = 0;

        _playModeOption.Clear();
        _playModeOption.AddItem(nameof(CardPlayMode.Basic));
        _playModeOption.AddItem(nameof(CardPlayMode.Enhanced));
        _playModeOption.AddItem(nameof(CardPlayMode.Sideways));
        _playModeOption.Select(0);

        _sidewaysResourceOption.Clear();
        _sidewaysResourceOption.AddItem("movement");
        _sidewaysResourceOption.AddItem("attack");
        _sidewaysResourceOption.AddItem("block");
        _sidewaysResourceOption.AddItem("influence");
        _sidewaysResourceOption.Select(0);

        _eventLog.ScrollFollowing = true;
        _playButton.Disabled = true;
    }

    private void WireSignals()
    {
        _startRoundButton.Pressed += OnStartRoundPressed;
        _drawButton.Pressed += OnDrawPressed;
        _endTurnButton.Pressed += OnEndTurnPressed;
        _playButton.Pressed += OnPlayPressed;
        _handList.ItemSelected += OnHandItemSelected;
    }

    private void TryResolveGameManager()
    {
        if (_gameManager == null)
        {
            _gameManager = GetNodeOrNull<GameManager>(GameManagerPath);
            if (_gameManager == null)
            {
                SetControlsEnabled(false);
                AddLogLine("ERROR: GameManager not found at configured path.");
                return;
            }
        }

        if (!_gameManager.IsSessionReady)
        {
            SetControlsEnabled(false);
            return;
        }

        SetControlsEnabled(true);
        if (!_subscribed && _gameManager.EventBus != null)
        {
            SubscribeToEvents(_gameManager.EventBus);
            _subscribed = true;
            AddLogLine("Session ready. Card panel subscribed to events.");
        }
    }

    private void SubscribeToEvents(EventBus bus)
    {
        bus.Subscribe<EvtRoundStarted>(OnRoundStarted);
        bus.Subscribe<EvtDeckReshuffled>(OnDeckReshuffled);
        bus.Subscribe<EvtCardsDrawn>(OnCardsDrawn);
        bus.Subscribe<EvtCardsMoved>(OnCardsMoved);
        bus.Subscribe<EvtCardPlayed>(OnCardPlayed);
        bus.Subscribe<EvtTurnResourcesChanged>(OnTurnResourcesChanged);
        bus.Subscribe<EvtTurnEnded>(OnTurnEnded);
        bus.Subscribe<EvtPlayerReputationChanged>(OnReputationChanged);
    }

    private void SetControlsEnabled(bool enabled)
    {
        _startRoundButton.Disabled = !enabled;
        _drawCountSpinBox.Editable = enabled;
        _drawButton.Disabled = !enabled;
        _endTurnButton.Disabled = !enabled;
        _handList.MouseFilter = enabled ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        _playModeOption.Disabled = !enabled;
        _choiceIndexSpinBox.Editable = enabled;
        _sidewaysResourceOption.Disabled = !enabled;
        _playButton.Disabled = !enabled || _selectedCardId == null;
    }

    private void RefreshUi()
    {
        if (_gameManager?.IsSessionReady != true || _gameManager.State == null)
        {
            _phaseLabel.Text = "Phase: Session not ready";
            _deckSummaryLabel.Text = "Deck: -";
            _resourcesLabel.Text = "Resources: -";
            _handList.Clear();
            _playButton.Disabled = true;
            return;
        }

        GameState state = _gameManager.State;
        PlayerState? player = _gameManager.GetPlayerState(_playerId);
        if (player == null)
        {
            _phaseLabel.Text = "Phase: Player 0 not found";
            _deckSummaryLabel.Text = "Deck: -";
            _resourcesLabel.Text = "Resources: -";
            _handList.Clear();
            _playButton.Disabled = true;
            return;
        }

        _phaseLabel.Text = $"Round {state.RoundNumber} | Phase {state.CurrentPhase} | Active {state.ActivePlayerId}";
        _deckSummaryLabel.Text = $"Deck D:{player.DrawPile.Count} H:{player.Hand.Count} P:{player.PlayArea.Count} X:{player.DiscardPile.Count}";
        _resourcesLabel.Text =
            $"Res Mv:{player.TurnResources.Movement} At:{player.TurnResources.Attack} Bl:{player.TurnResources.Block} " +
            $"If:{player.TurnResources.Influence} He:{player.TurnResources.Healing} Rep:{player.Reputation}";

        RefreshHandList(player);
    }

    private void RefreshHandList(PlayerState player)
    {
        _handList.Clear();
        int selectedIndex = -1;

        for (int i = 0; i < player.Hand.Count; i++)
        {
            CardInstance card = player.Hand[i];
            _handList.AddItem(CardPlayPanelHelpers.FormatHandLabel(card));
            _handList.SetItemMetadata(i, card.Id.Value);

            if (_selectedCardId.HasValue && _selectedCardId.Value == card.Id)
            {
                selectedIndex = i;
            }
        }

        if (selectedIndex >= 0)
        {
            _handList.Select(selectedIndex);
        }
        else
        {
            _selectedCardId = null;
        }

        _playButton.Disabled = _selectedCardId == null;
    }

    private void OnStartRoundPressed()
    {
        if (_gameManager == null)
        {
            return;
        }

        CommandResult result = _gameManager.StartRound();
        AddCommandResult("StartRound", result);
        RefreshUi();
    }

    private void OnDrawPressed()
    {
        if (_gameManager == null)
        {
            return;
        }

        int drawCount = (int)_drawCountSpinBox.Value;
        CommandResult result = _gameManager.DrawCards(_playerId, drawCount);
        AddCommandResult($"Draw({drawCount})", result);
        RefreshUi();
    }

    private void OnEndTurnPressed()
    {
        if (_gameManager == null)
        {
            return;
        }

        CommandResult result = _gameManager.EndTurn(_playerId);
        AddCommandResult("EndTurn", result);
        RefreshUi();
    }

    private void OnPlayPressed()
    {
        if (_gameManager == null || _selectedCardId == null)
        {
            return;
        }

        CardPlayMode mode = (CardPlayMode)_playModeOption.Selected;
        int rootChoiceIndex = (int)_choiceIndexSpinBox.Value;
        string sidewaysResource = _sidewaysResourceOption.GetItemText(_sidewaysResourceOption.Selected);

        PlayCardRequest request = CardPlayPanelHelpers.BuildPlayRequest(
            _playerId,
            _selectedCardId.Value,
            mode,
            rootChoiceIndex,
            sidewaysResource);

        CommandResult result = _gameManager.PlayCard(request);
        AddCommandResult($"Play({mode})", result);
        RefreshUi();
    }

    private void OnHandItemSelected(long index)
    {
        if (index < 0)
        {
            _selectedCardId = null;
            _playButton.Disabled = true;
            return;
        }

        Variant metadata = _handList.GetItemMetadata((int)index);
        _selectedCardId = new CardInstanceId(metadata.AsInt32());
        _playButton.Disabled = false;
    }

    private void AddCommandResult(string action, CommandResult result)
    {
        if (result.IsSuccess)
        {
            AddLogLine($"{action}: OK ({result.Events?.Count ?? 0} events)");
            return;
        }

        ValidationError? firstError = result.Errors?.FirstOrDefault();
        string message = firstError == null
            ? result.GetErrorSummary()
            : $"{firstError.Code}: {firstError.Message}";
        AddLogLine($"{action}: FAIL {message}");
    }

    private void AddLogLine(string line)
    {
        _logLines.Add(line);
        IReadOnlyList<string> trimmed = CardPlayPanelHelpers.TrimEventLog(_logLines, 30);
        _logLines.Clear();
        _logLines.AddRange(trimmed);
        _eventLog.Text = string.Join('\n', _logLines);
    }

    private void OnRoundStarted(EvtRoundStarted evt)
    {
        AddLogLine($"EvtRoundStarted #{evt.RoundNumber}");
        RefreshUi();
    }

    private void OnDeckReshuffled(EvtDeckReshuffled evt)
    {
        AddLogLine($"EvtDeckReshuffled P{evt.PlayerId.Value} count={evt.CardCount}");
        RefreshUi();
    }

    private void OnCardsDrawn(EvtCardsDrawn evt)
    {
        AddLogLine($"EvtCardsDrawn P{evt.PlayerId.Value} count={evt.CardInstanceIds.Count}");
        RefreshUi();
    }

    private void OnCardsMoved(EvtCardsMoved evt)
    {
        AddLogLine($"EvtCardsMoved P{evt.PlayerId.Value} count={evt.Changes.Count}");
        RefreshUi();
    }

    private void OnCardPlayed(EvtCardPlayed evt)
    {
        AddLogLine($"EvtCardPlayed {evt.CardId} mode={evt.Mode}");
        RefreshUi();
    }

    private void OnTurnResourcesChanged(EvtTurnResourcesChanged evt)
    {
        AddLogLine($"EvtTurnResourcesChanged P{evt.PlayerId.Value}");
        RefreshUi();
    }

    private void OnTurnEnded(EvtTurnEnded evt)
    {
        AddLogLine($"EvtTurnEnded P{evt.PlayerId.Value} roundEnded={evt.RoundEnded}");
        RefreshUi();
    }

    private void OnReputationChanged(EvtPlayerReputationChanged evt)
    {
        AddLogLine($"EvtReputationChanged P{evt.PlayerId.Value} {evt.PreviousReputation}->{evt.CurrentReputation}");
        RefreshUi();
    }
}
