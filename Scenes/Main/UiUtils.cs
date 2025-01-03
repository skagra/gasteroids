using System;

namespace Asteroids;

public class UiUtils
{
    [Flags]
    public enum ViewableElements
    {
        None = 0b_0000_0000,
        SplashScreen = 0b_0000_0001,
        HelpLabel = 0b_0000_0010,
        StartLabel = 0b_0000_0100,
        GameOverLabel = 0b0000_1000,
        HighScoreTable = 0b0001_0000,
        HelpDialog = 0b0010_0000,
        SettingsDialog = 0b0100_0000,
        FadingOverlay = 0b1000_0000
    }

    private readonly Splash _splashScreen;
    private readonly Ui _ui;
    private readonly FadingPanelContainer _fadingOverlay;
    private readonly HighScoreTable _highScoreTable;
    private readonly HelpDialog _helpDialog;
    private readonly GameSettingsDialog _settingsDialog;

    public UiUtils(Splash splashScreen, Ui ui, FadingPanelContainer fadingOverlay,
        HighScoreTable highScoreTable, HelpDialog helpDialog, GameSettingsDialog settingsDialog)
    {
        _splashScreen = splashScreen;
        _ui = ui;
        _fadingOverlay = fadingOverlay;
        _highScoreTable = highScoreTable;
        _helpDialog = helpDialog;
        _settingsDialog = settingsDialog;
    }

    public void ShowAndHide(ViewableElements flags, ViewableElements immediate = ViewableElements.None)
    {
        if ((flags & ViewableElements.SplashScreen) != 0)
        {
            _splashScreen.Show();
        }
        else
        {
            _splashScreen.Hide();
        }

        if ((flags & ViewableElements.HelpLabel) != 0)
        {
            _ui.ShowHelpLabel();
        }
        else
        {
            _ui.HideHelpLabel();
        }

        if ((flags & ViewableElements.StartLabel) != 0)
        {
            _ui.ShowStartLabel();
        }
        else
        {
            _ui.HideStartLabel();
        }

        if ((flags & ViewableElements.GameOverLabel) != 0)
        {
            _ui.ShowGameOverLabel();
        }
        else
        {
            _ui.HideGameOverLabel();
        }

        if ((flags & ViewableElements.HighScoreTable) != 0)
        {
            _highScoreTable.Show((immediate & ViewableElements.HighScoreTable) != 0);
        }
        else
        {
            _highScoreTable.Hide((immediate & ViewableElements.HighScoreTable) != 0);
        }

        if ((flags & ViewableElements.HelpDialog) != 0)
        {
            _helpDialog.Show((immediate & ViewableElements.HelpDialog) != 0);
        }
        else
        {
            _helpDialog.Hide((immediate & ViewableElements.HelpDialog) != 0);
        }

        if ((flags & ViewableElements.SettingsDialog) != 0)
        {
            _settingsDialog.Show((immediate & ViewableElements.SettingsDialog) != 0);
        }
        else
        {
            _settingsDialog.Hide((immediate & ViewableElements.SettingsDialog) != 0);
        }

        if ((flags & ViewableElements.FadingOverlay) != 0)
        {
            _fadingOverlay.Show((immediate & ViewableElements.FadingOverlay) != 0);
        }
        else
        {
            _fadingOverlay.Hide((immediate & ViewableElements.FadingOverlay) != 0);
        }
    }

}