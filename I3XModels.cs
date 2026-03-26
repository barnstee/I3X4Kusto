namespace I3X4Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public sealed record I3xNamespace(
        [property: JsonPropertyName("uri")] string Uri,
        [property: JsonPropertyName("displayName")] string DisplayName);

    public sealed record I3xObjectType(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("namespaceUri")] string NamespaceUri,
        [property: JsonPropertyName("schema")] Dictionary<string, object> Schema);

    public sealed record I3xRelationshipType(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("namespaceUri")] string NamespaceUri,
        [property: JsonPropertyName("reverseOf")] string ReverseOf);

    public sealed record I3xObject(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("typeId")] string TypeId,
        [property: JsonPropertyName("isComposition")] bool IsComposition,
        [property: JsonPropertyName("namespaceUri")] string NamespaceUri,
        [property: JsonPropertyName("parentId")] string ParentId = null,
        [property: JsonPropertyName("relationships")] Dictionary<string, object> Relationships = null);

    public sealed record ElementIdQuery(
        [property: JsonPropertyName("elementIds")] string[] ElementIds);

    public sealed record GetObjectsRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("includeMetadata")] bool IncludeMetadata = false);

    public sealed record GetRelatedObjectsRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("relationshiptype")] string RelationshipType = null,
        [property: JsonPropertyName("includeMetadata")] bool IncludeMetadata = false);

    public sealed record I3xValueQueryRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("maxDepth")] int MaxDepth = 1);

    public sealed record I3xHistoryQueryRequest(
        [property: JsonPropertyName("elementIds")] string[] ElementIds,
        [property: JsonPropertyName("startTime")] string StartTime = null,
        [property: JsonPropertyName("endTime")] string EndTime = null,
        [property: JsonPropertyName("maxDepth")] int MaxDepth = 1);

    public sealed record I3xValueResult(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
        [property: JsonPropertyName("attributes")] Dictionary<string, object> Attributes);

    public sealed record I3xHistoryResult(
        [property: JsonPropertyName("elementId")] string ElementId,
        [property: JsonPropertyName("samples")] IReadOnlyList<I3xValueResult> Samples);
}
