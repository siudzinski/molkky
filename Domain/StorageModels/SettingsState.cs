namespace molkky.Domain.StorageModels;

public record SettingsState(
    MaximumPointsStrategies MaximumPointsStrategy,
    MissedThrowsStrategies MissedThrowsStrategy);