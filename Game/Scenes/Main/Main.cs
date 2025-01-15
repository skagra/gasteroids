using System;
using Godot;

namespace Asteroids;

using static Asteroids.UiUtils;
using SettingsPresets = GameSettingsPresets.SettingsPresets;

public partial class Main : Node
{
    private const string _SETTINGS_SAVE_PATH = "user://settings.json";
    private const string _HIGH_SCORE_SAVE_PATH = "user://highscores.json";

    // Scene references
    private GameSettingsDialog _settingsDialog;
    private HelpDialog _helpDialog;
    private HighScoreTable _highScoreTable;
    private FadingPanelContainer _fadingOverlay;
    private Splash _splashScreen;
    private MainAnimationPlayer _mainAnimationPlayer;
    private Ui _ui;
    private EnterHighScore _enterHighScore;
    private Scores _scores;
    private EventHub _eventHub;
    private GamePlayController _gamePlayController;

    // State
    private int _asteroidsCurrentInitialQuantity;
    private int _extraLifeThresholdNext;

    // Game state machine
    private enum PreGameState { Passive, Playing, ShowingConfigDialog, ShowingHelpDialog, EnteringHighScore };
    private PreGameState _preGameState;

    // Game settings
    // private GameSettingsBridge _settingsBridge; TODO
    private GameSettings _gameSettings;

    // UI hide and show utils
    private UiUtils _uiUtils;

    public override void _Ready()
    {
        Logger.I.Debug("Main scene ready");

        // Scene references
        SetupSceneReferences();

        // Signals
        SetupSceneSignals();

        // Set up UI utils
        _uiUtils = new UiUtils(_splashScreen, _ui, _fadingOverlay, _highScoreTable, _helpDialog, _settingsDialog);

        // Load configuration
        _gameSettings = GameSettingsPersistence.Load(_SETTINGS_SAVE_PATH) ?? GameSettingsPresets.GetSettings(SettingsPresets.Normal);
        Resources.SwitchTheme(_gameSettings.Theme);

        // Load high scores
        var highScores = HighScorePersistence.Load(_HIGH_SCORE_SAVE_PATH);
        if (highScores != null)
        {
            _highScoreTable.SetHighScores(highScores);
        }
        _ui.HighScore = _highScoreTable.HighScore;

        // Screen overlay to dim background when not playing
        _fadingOverlay.SpeedScale = 0.1f;

        // Splash screen followed by main animation loop
        _mainAnimationPlayer.PlaySplash();

        // Hide the cursor
        HideMouse();

        _preGameState = PreGameState.Passive;
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("Quit"))
        {
            GetTree().Quit();
        }

        if (_preGameState == PreGameState.Passive)
        {
            if (inputEvent.IsActionPressed("Start"))
            {
                NewGame();
            }
            else if (inputEvent.IsActionPressed("Config"))
            {
                ShowConfigDialog();
            }
            else if (inputEvent.IsActionPressed("Help"))
            {
                ShowHelpDialog();
            }
        }
    }

    public override void _Notification(int what)
    {
        /// Maybe better to
        switch ((long)what)
        {
            case NotificationApplicationFocusIn:
                if (_preGameState != PreGameState.ShowingConfigDialog && _preGameState != PreGameState.ShowingHelpDialog)
                {
                    HideMouse();
                }
                else
                {
                    ShowMouse(false);
                }
                break;
            case NotificationApplicationFocusOut:
                ShowMouse(false);
                break;
            default:
                break;
        }
    }

    private void ShowMouse(bool warp)
    {
        if (warp)
        {
            GetViewport().WarpMouse(Screen.Centre);
        }
        Input.MouseMode = Input.MouseModeEnum.Visible;

    }

    private static void HideMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    // Setup -->

    private void SetupSceneReferences()
    {
        // Get references too all required scenes
        _settingsDialog = GetNode<GameSettingsDialog>("Game Settings Dialog") ?? throw new NullReferenceException("Game Settings Dialog not found");
        _helpDialog = GetNode<HelpDialog>("Help Dialog") ?? throw new NullReferenceException("Help Dialog not found");
        _highScoreTable = GetNode<HighScoreTable>("HighScoreTable") ?? throw new NullReferenceException("HighScoreTable not found");
        _fadingOverlay = (FadingPanelContainer)FindChild("FadingOverlay") ?? throw new NullReferenceException("FadingOverlay not found");
        _splashScreen = (Splash)FindChild("Splash") ?? throw new NullReferenceException("Splash not found");
        _mainAnimationPlayer = (MainAnimationPlayer)FindChild("MainAnimationPlayer") ?? throw new NullReferenceException("MainAnimationPlayer not found");
        _ui = (Ui)FindChild("UI") ?? throw new NullReferenceException("UI not found");
        _enterHighScore = (EnterHighScore)FindChild("EnterHighScore") ?? throw new NullReferenceException("EnterHighScore not found");
        _scores = (Scores)FindChild("Scores") ?? throw new NullReferenceException("Scores not found"); // ??
        _eventHub = (EventHub)FindChild("EventHub") ?? throw new NullReferenceException("EventHub not found");
        _gamePlayController = (GamePlayController)FindChild("GamePlayController") ?? throw new NullReferenceException("GamePlayController not found");
    }

    private void SetupSceneSignals()
    {
        // Configuration settings
        _settingsDialog.OkPressed += SettingsDialogOnOkPressed;
        _settingsDialog.Cancel += SettingsDialogOnCancel;

        // Controls
        _helpDialog.OkPressed += HelpDialogOnOkPressed;

        // High score name entered
        _enterHighScore.NameEntered += HighScoreOnNameEntered;

        // Game over
        _gamePlayController.GameOver += OnGameOver;
    }

    // <-- Setup

    // Game state control ->

    private void NewGame()
    {
        _mainAnimationPlayer.Stop();
        _uiUtils.ShowAndHide(ViewableElements.None);
        _gamePlayController.NewGame(_gameSettings);
        _preGameState = PreGameState.Playing;
    }

    private void OnGameOver()
    {
        // Show the fading overlay
        _uiUtils.ShowAndHide(ViewableElements.FadingOverlay);

        // Disable all new sound fxs
        Resources.EnableNewSoundFx(false);

        // Check if the score is high enough to be included in the high score table
        if (_highScoreTable.IsEligibleForInclusion(_gamePlayController.Score))
        {
            _ui.ShowGameOverLabel();
            _enterHighScore.Show();
            Logger.I.Debug("Setting game state to {0}", PreGameState.EnteringHighScore);
            _preGameState = PreGameState.EnteringHighScore;
        }
        else
        {
            _mainAnimationPlayer.PlayGameOver();
            _preGameState = PreGameState.Passive;
        }
    }

    private void HighScoreOnNameEntered(string name)
    {
        _highScoreTable.AddScore(name, _gamePlayController.Score);
        HighScorePersistence.Save(_highScoreTable.GetScores(), _HIGH_SCORE_SAVE_PATH);
        _enterHighScore.Hide();

        _uiUtils.ShowAndHide(ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayHighScore();

        _preGameState = PreGameState.Passive;
    }

    // <-- Game state control

    // Configuration -->

    private void ShowConfigDialog()
    {
        ShowMouse(true);
        _mainAnimationPlayer.Stop();
        _settingsDialog.ActiveSettings = _gameSettings;
        _uiUtils.ShowAndHide(ViewableElements.SettingsDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        Logger.I.Debug("Entering game state {0}", PreGameState.ShowingConfigDialog);
        _preGameState = PreGameState.ShowingConfigDialog;
    }

    private void SettingsDialogOnOkPressed()
    {
        HideMouse();
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _gameSettings = new GameSettings(_settingsDialog.ActiveSettings);
        Resources.SwitchTheme(_gameSettings.Theme);
        GameSettingsPersistence.Save(_settingsDialog.ActiveSettings, _SETTINGS_SAVE_PATH);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Entering game state {0}", PreGameState.Passive);
        _preGameState = PreGameState.Passive;
    }

    private void SettingsDialogOnCancel()
    {
        HideMouse();
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Entering game state {0}", PreGameState.Passive);
        _preGameState = PreGameState.Passive;
    }

    // <-- Configuration

    // Help dialog -->

    private void ShowHelpDialog()
    {
        ShowMouse(true);
        _mainAnimationPlayer.Stop();
        _uiUtils.ShowAndHide(ViewableElements.HelpDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        Logger.I.Debug("Entering game state {0}", PreGameState.ShowingHelpDialog);
        _preGameState = PreGameState.ShowingHelpDialog;
    }

    private void HelpDialogOnOkPressed()
    {
        HideMouse();
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Entering game state {0}", PreGameState.Passive);
        _preGameState = PreGameState.Passive;
    }

    // <-- Help dialog
}
