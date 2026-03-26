using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public ActionResult<IEnumerable<I3xObjectType>> GetObjectTypes() => Ok(new List<I3xObjectType>() { new I3xObjectType("test1", "test2", "test3") });

        [HttpPost("query")]
        public ActionResult<IEnumerable<I3xObjectType>> QueryByElementId([FromBody] ElementIdQuery query)
        {
            return Ok(new List<I3xObjectType>() { new I3xObjectType("test1", "test2", "test3") });
        }
    }
}
