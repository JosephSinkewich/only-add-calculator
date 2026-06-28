using System.Text.Json;
using System.Text.Json.Serialization;
using OnlyAddCalculator.Application;

namespace OnlyAddCalculator.Persistence;

public sealed class JsonAppStateStore : IAppStateStore
{
    private const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _filePath;

    public JsonAppStateStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        _filePath = filePath;
    }

    public AppState Load()
    {
        if (!File.Exists(_filePath))
        {
            return AppState.Empty;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var dto = JsonSerializer.Deserialize<PersistedAppStateDto>(json, _serializerOptions);

            if (dto is null || dto.SchemaVersion != CurrentSchemaVersion)
            {
                return AppState.Empty;
            }

            return new AppState
            {
                CurrentInput = dto.CurrentInput ?? string.Empty,
                History = dto.History?
                    .Where(static item => item is not null)
                    .Select(static item => new HistoryItem
                    {
                        Input = item.Input ?? string.Empty,
                        Result = item.Result ?? string.Empty,
                        IsError = item.IsError,
                    })
                    .ToArray() ?? [],
            };
        }
        catch (JsonException)
        {
            return AppState.Empty;
        }
        catch (IOException)
        {
            return AppState.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return AppState.Empty;
        }
    }

    public void Save(AppState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var directoryPath = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var dto = new PersistedAppStateDto
        {
            SchemaVersion = CurrentSchemaVersion,
            CurrentInput = state.CurrentInput,
            History = state.History.Select(static item => new PersistedHistoryItemDto
            {
                Input = item.Input,
                Result = item.Result,
                IsError = item.IsError,
            }).ToArray(),
        };

        var json = JsonSerializer.Serialize(dto, _serializerOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class PersistedAppStateDto
    {
        public int SchemaVersion { get; init; }

        public string? CurrentInput { get; init; }

        public PersistedHistoryItemDto[]? History { get; init; }
    }

    private sealed class PersistedHistoryItemDto
    {
        public string? Input { get; init; }

        public string? Result { get; init; }

        public bool IsError { get; init; }
    }
}
