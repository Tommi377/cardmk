using System.IO;

namespace RealMK;

/// <summary>
/// Creates game sessions from content sources.
/// </summary>
public static class GameSessionFactory
{
    /// <summary>
    /// Creates a game session by loading content from the provided path.
    /// </summary>
    public static IGameSession CreateFromContentPath(string contentPath, ulong seed, bool validate = true)
    {
        var loader = new ContentLoader();
        ContentDatabase contentDatabase = Directory.Exists(contentPath)
            ? loader.LoadAll(contentPath, validate)
            : new ContentDatabase();

        return new GameSession(contentDatabase, seed);
    }
}
