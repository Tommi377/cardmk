using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class ArchitectureTests
{
    [Fact]
    public void DomainFolders_DoNotReferenceGodotNamespace()
    {
        string root = GetRepositoryRoot();
        string[] domainFolders =
        {
            Path.Combine(root, "scripts", "core"),
            Path.Combine(root, "scripts", "map"),
            Path.Combine(root, "scripts", "cards"),
            Path.Combine(root, "scripts", "commands"),
            Path.Combine(root, "scripts", "events"),
            Path.Combine(root, "scripts", "mana")
        };

        var offenders = new List<string>();

        foreach (string folder in domainFolders)
        {
            foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                if (File.ReadAllText(file).Contains("using Godot;", StringComparison.Ordinal))
                {
                    offenders.Add(Path.GetRelativePath(root, file));
                }
            }
        }

        Assert.True(offenders.Count == 0, $"Domain files referencing Godot: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void GameManager_DoesNotInstantiateContentParserOrMapGenerator()
    {
        string root = GetRepositoryRoot();
        string gameManagerPath = Path.Combine(root, "scenes", "game_manager", "GameManager.cs");
        string source = File.ReadAllText(gameManagerPath);

        string[] forbidden =
        {
            "new MapGenerator(",
            "new ContentLoader(",
            ".LoadAll(",
            "DateTime.UtcNow",
            "EventIndex = 0"
        };

        var matches = forbidden.Where(source.Contains).ToList();
        Assert.True(matches.Count == 0, $"GameManager contains forbidden orchestration tokens: {string.Join(", ", matches)}");
    }

    private static string GetRepositoryRoot()
    {
        string? dir = AppContext.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            if (dir == null)
            {
                break;
            }

            if (File.Exists(Path.Combine(dir, "RealMK.sln")))
            {
                return dir;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }
}
