namespace Molkky.Domain.StorageModels;

public record SettingsState(
    MaximumPointsStrategies MaximumPointsStrategy,
    MissedThrowsStrategies MissedThrowsStrategy);
