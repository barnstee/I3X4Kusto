namespace I3X4Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public sealed record I3xNamespace(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description = null);

    public sealed record I3xObjectType(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("namespaceElementId")] string NamespaceElementId);

    public sealed record I3xRelationshipType(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("name")] string Name);

    public sealed record I3xObject(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("objectTypeElementId")] string ObjectTypeElementId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("attributes")] Dictionary<string, object> Attributes);

    public sealed record ElementIdQuery(
        [property: JsonPropertyName("elementIds")] string[] ElementIds);

    public sealed record ListObjectsRequest(
        [property: JsonPropertyName("top")] int Top);

    public sealed record GetRelatedObjectsRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("relationshiptype")] string RelationshipType = null,
        [property: JsonPropertyName("includeMetadata")] bool IncludeMetadata = false);

    public sealed record I3xValueQueryRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds);

    public sealed record I3xHistoryQueryRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("startTime")] DateTimeOffset StartTime,
        [property: JsonPropertyName("endTime")] DateTimeOffset EndTime);

    public sealed record I3xValueResult(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
        [property: JsonPropertyName("attributes")] Dictionary<string, object> Attributes);

    public sealed record I3xHistoryResult(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("samples")] IReadOnlyList<I3xValueResult> Samples);
}
