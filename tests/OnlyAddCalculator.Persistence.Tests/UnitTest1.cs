using System.Text.Json;
using OnlyAddCalculator.Application;
using OnlyAddCalculator.Persistence;

namespace OnlyAddCalculator.Persistence.Tests;

public sealed class JsonAppStateStoreTests
{
    [Fact]
    public void Load_ReturnsEmptyState_WhenFileDoesNotExist()
    {
        var store = new JsonAppStateStore(CreateStateFilePath());

        var state = store.Load();

        Assert.Equal(string.Empty, state.CurrentInput);
        Assert.Empty(state.History);
    }

    [Fact]
    public void Save_CreatesFileWithSchemaVersion()
    {
        var filePath = CreateStateFilePath();
        var store = new JsonAppStateStore(filePath);

        store.Save(new AppState
        {
            CurrentInput = "54+21",
            History =
            [
                new HistoryItem { Input = "54+21", Result = "75", IsError = false },
            ],
        });

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("54+21", root.GetProperty("currentInput").GetString());
        Assert.Single(root.GetProperty("history").EnumerateArray());
    }

    [Fact]
    public void Load_ReturnsSavedState()
    {
        var filePath = CreateStateFilePath();
        var store = new JsonAppStateStore(filePath);
        var expectedState = new AppState
        {
            CurrentInput = "45+-88",
            History =
            [
                new HistoryItem { Input = "54+21", Result = "75", IsError = false },
                new HistoryItem { Input = "45+-88", Result = "Error", IsError = true },
            ],
        };

        store.Save(expectedState);

        var loadedState = store.Load();

        Assert.Equal("45+-88", loadedState.CurrentInput);
        Assert.Collection(
            loadedState.History,
            first =>
            {
                Assert.Equal("54+21", first.Input);
                Assert.Equal("75", first.Result);
                Assert.False(first.IsError);
            },
            second =>
            {
                Assert.Equal("45+-88", second.Input);
                Assert.Equal("Error", second.Result);
                Assert.True(second.IsError);
            });
    }

    [Fact]
    public void Load_PreservesStateAcrossStoreInstances()
    {
        var filePath = CreateStateFilePath();
        var firstStore = new JsonAppStateStore(filePath);
        firstStore.Save(new AppState
        {
            CurrentInput = "55+13",
            History =
            [
                new HistoryItem { Input = "55+13", Result = "68", IsError = false },
            ],
        });

        var secondStore = new JsonAppStateStore(filePath);

        var loadedState = secondStore.Load();

        Assert.Equal("55+13", loadedState.CurrentInput);
        var item = Assert.Single(loadedState.History);
        Assert.Equal("68", item.Result);
    }

    [Fact]
    public void Load_ReturnsEmptyState_WhenJsonIsMalformed()
    {
        var filePath = CreateStateFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ not valid json");
        var store = new JsonAppStateStore(filePath);

        var state = store.Load();

        Assert.Equal(string.Empty, state.CurrentInput);
        Assert.Empty(state.History);
    }

    [Fact]
    public void Load_ReturnsEmptyState_WhenSchemaVersionIsUnsupported()
    {
        var filePath = CreateStateFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, """{"schemaVersion":2,"currentInput":"54+21","history":[]}""");
        var store = new JsonAppStateStore(filePath);

        var state = store.Load();

        Assert.Equal(string.Empty, state.CurrentInput);
        Assert.Empty(state.History);
    }

    private static string CreateStateFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "OnlyAddCalculator.Tests", Guid.NewGuid().ToString("N"), "state.json");
    }
}