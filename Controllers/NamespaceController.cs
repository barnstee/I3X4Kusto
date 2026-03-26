using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace I3X4Kusto.Controllers
{
    [ApiController]
    [Route("v0/namespaces")]
    public sealed class NamespacesController : ControllerBase
    {
        private readonly ADXDataService _kusto;

        public NamespacesController(ADXDataService kusto)
        {
            _kusto = kusto;
            _kusto.Connect();
        }

        [HttpGet]
        public ActionResult<IEnumerable<I3xNamespace>> GetNamespaces()
        {
            string query = "opcua_metadata_lkv\r\n"
                         + "| distinct NamespaceUri";

            var rows = _kusto.RunQueryRows(query);

            var results = rows.Select(r => new I3xNamespace(
                Str(r, "NamespaceUri"),
                ExtractNameFromUri(Str(r, "NamespaceUri"))
            )).ToList();

            return Ok(results);
        }

        private static string ExtractNameFromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return "";
            var trimmed = uri.TrimEnd('/');
            int lastSlash = trimmed.LastIndexOf('/');
            return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
        }

        private static string Str(Dictionary<string, object> row, string key) =>
            row.TryGetValue(key, out var v) ? v?.ToString() ?? "" : "";
    }
}
