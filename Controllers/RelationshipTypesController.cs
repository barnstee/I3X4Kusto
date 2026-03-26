using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace I3X4Kusto.Controllers
{
    [ApiController]
    [Route("v0/relationshiptypes")]
    public sealed class RelationshipTypesController : ControllerBase
    {
        private readonly ADXDataService _kusto;

        public RelationshipTypesController(ADXDataService kusto)
        {
            _kusto = kusto;
            _kusto.Connect();
        }

        [HttpGet]
        public ActionResult<IEnumerable<I3xRelationshipType>> GetRelationshipTypes() => Ok(new List<I3xRelationshipType>() { new I3xRelationshipType("test1", "test2") });

        [HttpPost("query")]
        public ActionResult<IEnumerable<I3xRelationshipType>> QueryByElementId([FromBody] ElementIdQuery query)
        {
            return Ok(new List<I3xRelationshipType>() { new I3xRelationshipType("test1", "test2") });
        }
    }
}
