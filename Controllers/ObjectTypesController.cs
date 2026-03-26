using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace I3X4Kusto.Controllers
{
    [ApiController]
    [Route("v0/objecttypes")]
    public sealed class ObjectTypesController : ControllerBase
    {
        private readonly ADXDataService _kusto;

        public ObjectTypesController(ADXDataService kusto)
        {
            _kusto = kusto;
            _kusto.Connect();
        }

        [HttpGet]
        public ActionResult<IEnumerable<I3xObjectType>> GetObjectTypes()
        {
            string query = "opcua_metadata_lkv\r\n"
                         + "| distinct Type, NamespaceUri\r\n"
                         + "| project Type, NamespaceUri";

            var rows = _kusto.RunQueryRows(query);

            var results = rows.Select(r => new I3xObjectType(
                Str(r, "Type"),
                Str(r, "Type"),
                Str(r, "NamespaceUri"),
                new Dictionary<string, object>()
            )).ToList();

            return Ok(results);
        }

        [HttpPost("query")]
        public ActionResult<IEnumerable<I3xObjectType>> QueryByElementId([FromBody] ElementIdQuery query)
        {
            string inClause = ADXDataService.ToKqlStringList(query.ElementIds);

            string kql = "opcua_metadata_lkv\r\n"
                       + "| where Type in (" + inClause + ")\r\n"
                       + "| distinct Type, NamespaceUri\r\n"
                       + "| project Type, NamespaceUri";

            var rows = _kusto.RunQueryRows(kql);

            var results = rows.Select(r => new I3xObjectType(
                Str(r, "Type"),
                Str(r, "Type"),
                Str(r, "NamespaceUri"),
                new Dictionary<string, object>()
            )).ToList();

            return Ok(results);
        }

        private static string Str(Dictionary<string, object> row, string key) =>
            row.TryGetValue(key, out var v) ? v?.ToString() ?? "" : "";
    }
}
