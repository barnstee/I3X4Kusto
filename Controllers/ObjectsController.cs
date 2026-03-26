using I3X4Kusto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            return Ok(new List<I3xObject>());
        }

        [HttpPost("list")]
        public async Task<ActionResult<IEnumerable<I3xObject>>> ListObjects([FromBody] ListObjectsRequest req, CancellationToken ct)
        {
            return Ok(new List<I3xObject>() { new I3xObject("test1", "test2", "test3", new Dictionary<string, object>()) });
        }

        [HttpPost("related")]
        public async Task<ActionResult<IEnumerable<I3xObject>>> QueryRelatedObjects(
            [FromBody] GetRelatedObjectsRequest req,
            CancellationToken ct)
        {
            return Ok(new List<I3xObject>());
        }

        [HttpPost("value")]
        public async Task<ActionResult<IEnumerable<I3xValueResult>>> QueryValue([FromBody] I3xValueQueryRequest req, CancellationToken ct)
        {
            return Ok(new List<I3xValueResult>() { new I3xValueResult("test1", DateTime.UtcNow, new Dictionary<string, object>()) });
        }

        [HttpPost("history")]
        public async Task<ActionResult<IEnumerable<I3xHistoryResult>>> QueryHistory([FromBody] I3xHistoryQueryRequest req, CancellationToken ct)
        {
            return Ok(new List<I3xHistoryResult>() { new I3xHistoryResult("test1", new List<I3xValueResult>()) });
        }
    }
}