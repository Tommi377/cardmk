using System;
using Godot;

namespace RealMK;

/// <summary>
/// Main facade for interacting with the game.
/// Provides a clean API for the presentation layer to use.
/// </summary>
public partial class GameManager : Node
{
    private ContentDatabase? _contentDb;
    private EventBus? _eventBus;
    private CommandDispatcher? _dispatcher;

    public override void _Ready() {
        base._Ready();

        InitializeGame();
    }

    private void InitializeGame()
    {
        ShowLoading(true);

        _eventBus = new EventBus();
        _dispatcher= new CommandDispatcher(_eventBus);

        try
        {
            Log.Info("Initializing game...");

            // Create content database and load content
            var loader = new ContentLoader();
            string contentPath = ProjectSettings.GlobalizePath("res://content");

            // Try to load content, but don't fail if it doesn't exist
            if (DirAccess.DirExistsAbsolute(contentPath))
            {
                _contentDb = loader.LoadAll(contentPath, validate: false);
                Log.Debug($"GameRoot: Loaded content from {contentPath}");
            }
            else
            {
                Log.Error($"GameRoot: Content directory not found at {contentPath}, using empty database");
                _contentDb = new ContentDatabase();
            }


            Log.Info("Game initialized successfully");
        }
        catch (System.Exception ex)
        {
            Log.Error($"Failed to initialize game: {ex.Message}");
            Log.Error(ex.StackTrace!);
        }
        finally
        {
            ShowLoading(false);
        }
    }
    
    private void ShowLoading(bool show)
    {
        // TODO: Add loading
        // if (_loadingOverlay != null)
        // {
        //     _loadingOverlay.Visible = show;
        // }
    }
}