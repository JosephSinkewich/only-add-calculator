namespace OnlyAddCalculator.Application;

public interface IAppStateStore
{
    AppState Load();

    void Save(AppState state);
}
