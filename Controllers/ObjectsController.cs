using I3X4Kusto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace I3xKustoAdapter.Controllers
{
    [ApiController]
    [Route("v0/objects")]
    public sealed class ObjectsController : ControllerBase
    {
        private readonly ADXDataService _kusto;

        public ObjectsController(ADXDataService kusto)
        {
            _kusto = kusto;
            _kusto.Connect();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<I3xObject>>> GetObjects(
            [FromQuery] string typeId = null,
            [FromQuery] bool includeMetadata = false,
            CancellationToken ct = default)
        {
            string query;
            if (!string.IsNullOrEmpty(typeId))
            {
                // Objects whose telemetry includes the given variable type
                query = "opcua_metadata_lkv\r\n"
                      + "| where Type == \"" + typeId + "\"\r\n"
                      + "| project NodeId, DisplayName, Type, DataSetWriterID, NamespaceUri";
            }
            else
            {
                query = "opcua_metadata_lkv\r\n"
                      + "| project NodeId, DisplayName, Type, DataSetWriterID, NamespaceUri";
            }

            var rows = _kusto.RunQueryRows(query);

            if (includeMetadata)
            {
                return Ok(rows.Select(r =>
                {
                    var elementId = Str(r, "DataSetWriterID");
                    var attrs = GetLatestAttributes(elementId);
                    return new I3xObject(elementId, Str(r, "DisplayName"), Str(r, "Type"), false, Str(r, "NamespaceUri"), Str(r, "NodeId"), attrs);
                }).ToList());
            }

            return Ok(rows.Select(r => new I3xObject(
                Str(r, "DataSetWriterID"),
                Str(r, "DisplayName"),
                Str(r, "Type"),
                false,
                Str(r, "NamespaceUri"),
                Str(r, "NodeId")
            )).ToList());
        }

        [HttpPost("list")]
        public async Task<ActionResult<IEnumerable<I3xObject>>> ListObjects(
            [FromBody] GetObjectsRequest req,
            CancellationToken ct)
        {
            string inClause = ADXDataService.ToKqlStringList(req.ElementIds);

            string query = "opcua_metadata_lkv\r\n"
                         + "| where DataSetWriterID in (" + inClause + ")\r\n"
                         + "| project NodeId, DisplayName, Type, DataSetWriterID, NamespaceUri";

            var rows = _kusto.RunQueryRows(query);

            return Ok(rows.Select(r => new I3xObject(
                Str(r, "DataSetWriterID"),
                Str(r, "DisplayName"),
                Str(r, "Type"),
                false,
                Str(r, "NamespaceUri"),
                Str(r, "NodeId")
            )).ToList());
        }

        [HttpPost("related")]
        public async Task<ActionResult<IEnumerable<I3xObject>>> QueryRelatedObjects(
            [FromBody] GetRelatedObjectsRequest req,
            CancellationToken ct)
        {
            string inClause = ADXDataService.ToKqlStringList(req.ElementIds);

            string query = "opcua_metadata_lkv\r\n"
                         + "| where DataSetWriterID in (\r\n"
                         + "    opcua_metadata_lkv\r\n"
                         + "    | where Name in (" + inClause + ")\r\n"
                         + "    | distinct DataSetWriterID\r\n"
                         + ")\r\n"
                         + "| where Name !in (" + inClause + ")\r\n"
                         + "| distinct DataSetWriterID, Name\r\n"
                         + "| project ElementId = DataSetWriterID, ObjectTypeElementId = \"\", Name";

            var rows = _kusto.RunQueryRows(query);

            if (req.IncludeMetadata)
            {
                return Ok(rows.Select(r =>
                {
                    var elementId = Str(r, "ElementId");
                    var attrs = GetLatestAttributes(elementId);
                    return new I3xObject(elementId, Str(r, "Name"), Str(r, "ObjectTypeElementId"), false, "", null, attrs);
                }).ToList());
            }

            return Ok(rows.Select(r => new I3xObject(
                Str(r, "ElementId"),
                Str(r, "Name"),
                Str(r, "ObjectTypeElementId"),
                false,
                ""
            )).ToList());
        }

        [HttpPost("value")]
        public async Task<ActionResult<IEnumerable<I3xValueResult>>> QueryValue(
            [FromBody] I3xValueQueryRequest req,
            CancellationToken ct)
        {
            string inClause = ADXDataService.ToKqlStringList(req.ElementIds);

            string query = "opcua_telemetry\r\n"
                         + "| where DataSetWriterID in (" + inClause + ")\r\n"
                         + "| where Timestamp > now(- 1h)\r\n"
                         + "| summarize arg_max(Timestamp, Value) by DataSetWriterID, Name\r\n"
                         + "| project DataSetWriterID, Name, Timestamp, Value = todouble(Value)\r\n"
                         + "| sort by DataSetWriterID asc, Timestamp desc";

            var rows = _kusto.RunQueryRows(query);

            // Group by DataSetWriterID to build one I3xValueResult per object
            var results = rows
                .GroupBy(r => Str(r, "DataSetWriterID"))
                .Select(g =>
                {
                    var attrs = new Dictionary<string, object>();
                    DateTimeOffset latestTs = DateTimeOffset.MinValue;

                    foreach (var row in g)
                    {
                        attrs[Str(row, "Name")] = row.GetValueOrDefault("Value");

                        if (row.TryGetValue("Timestamp", out var ts) && ts is DateTime dt)
                        {
                            var offset = new DateTimeOffset(dt, TimeSpan.Zero);
                            if (offset > latestTs) latestTs = offset;
                        }
                    }

                    return new I3xValueResult(g.Key, latestTs, attrs);
                })
                .ToList();

            return Ok(results);
        }

        [HttpPost("history")]
        public async Task<ActionResult<IEnumerable<I3xHistoryResult>>> QueryHistory(
            [FromBody] I3xHistoryQueryRequest req,
            CancellationToken ct)
        {
            string inClause = ADXDataService.ToKqlStringList(req.ElementIds);
            string start = req.StartTime ?? DateTime.UtcNow.AddHours(-1).ToString("o");
            string end = req.EndTime ?? DateTime.UtcNow.ToString("o");

            string query = "opcua_telemetry\r\n"
                         + "| where DataSetWriterID in (" + inClause + ")\r\n"
                         + "| where Timestamp between (datetime(\"" + start + "\") .. datetime(\"" + end + "\"))\r\n"
                         + "| project DataSetWriterID, Name, Timestamp, Value = todouble(Value)\r\n"
                         + "| sort by DataSetWriterID asc, Timestamp desc";

            var rows = _kusto.RunQueryRows(query);

            var results = rows
                .GroupBy(r => Str(r, "DataSetWriterID"))
                .Select(g =>
                {
                    var samples = g
                        .GroupBy(r => r.GetValueOrDefault("Timestamp"))
                        .Select(tg =>
                        {
                            var attrs = new Dictionary<string, object>();
                            DateTimeOffset ts = DateTimeOffset.MinValue;

                            foreach (var row in tg)
                            {
                                attrs[Str(row, "Name")] = row.GetValueOrDefault("Value");

                                if (row.TryGetValue("Timestamp", out var t) && t is DateTime dt)
                                {
                                    ts = new DateTimeOffset(dt, TimeSpan.Zero);
                                }
                            }

                            return new I3xValueResult(g.Key, ts, attrs);
                        })
                        .OrderByDescending(s => s.Timestamp)
                        .ToList();

                    return new I3xHistoryResult(g.Key, samples);
                })
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Fetches the latest telemetry attributes for a single object.
        /// </summary>
        private Dictionary<string, object> GetLatestAttributes(string dataSetWriterId)
        {
            string query = "opcua_telemetry\r\n"
                         + "| where DataSetWriterID == \"" + dataSetWriterId + "\"\r\n"
                         + "| where Timestamp > now(- 1h)\r\n"
                         + "| summarize arg_max(Timestamp, Value) by Name\r\n"
                         + "| project Name, Value = todouble(Value)";

            var rows = _kusto.RunQueryRows(query);

            var attrs = new Dictionary<string, object>();
            foreach (var row in rows)
            {
                attrs[Str(row, "Name")] = row.GetValueOrDefault("Value");
            }

            return attrs;
        }

        private static string Str(Dictionary<string, object> row, string key) =>
            row.TryGetValue(key, out var v) ? v?.ToString() ?? "" : "";
    }
}