using System.Text.Json.Serialization;

namespace Molkky.Domain.Storage;

// The stored JSON, version 1. These shapes and the enum member names are the format: changing them
// means a new version, with the old one still loading (see StorageFormat).

internal sealed record GameDocumentV1(int Version, SettingsV1 Settings, PlayerV1[] Players, int[] Throws);

internal sealed record SettingsV1(MaximumPointsStrategies MaximumPoints, MissedThrowsStrategies MissedThrows);

internal sealed record PlayerV1(string Name, int Color);

internal sealed record SettingsDocumentV1(int Version, MaximumPointsStrategies MaximumPoints, MissedThrowsStrategies MissedThrows);

// Source-generated, so the format does not depend on reflection surviving the WebAssembly trimming.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(GameDocumentV1))]
[JsonSerializable(typeof(SettingsDocumentV1))]
internal sealed partial class StorageJsonContext : JsonSerializerContext;
