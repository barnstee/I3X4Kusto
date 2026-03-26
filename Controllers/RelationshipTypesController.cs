using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace I3X4Kusto.Controllers
{
    [ApiController]
    [Route("v0/relationshiptypes")]
    public sealed class RelationshipTypesController : ControllerBase
    {
        private readonly ADXDataService _kusto;

        // Well-known OPC UA reference types exposed through this adapter
        private static readonly List<I3xRelationshipType> KnownRelationshipTypes =
        [
            new("HasComponent", "HasComponent", "http://opcfoundation.org/UA/", "ComponentOf"),
            new("Organizes", "Organizes", "http://opcfoundation.org/UA/", "OrganizedBy"),
            new("HasProperty", "HasProperty", "http://opcfoundation.org/UA/", "PropertyOf"),
            new("HasSubtype", "HasSubtype", "http://opcfoundation.org/UA/", "SubtypeOf"),
            new("HasTypeDefinition", "HasTypeDefinition", "http://opcfoundation.org/UA/", "TypeDefinitionOf"),
            new("HasModellingRule", "HasModellingRule", "http://opcfoundation.org/UA/", "ModellingRuleOf"),
            new("HasEncoding", "HasEncoding", "http://opcfoundation.org/UA/", "EncodingOf"),
            new("HasDescription", "HasDescription", "http://opcfoundation.org/UA/", "DescriptionOf"),
            new("GeneratesEvent", "GeneratesEvent", "http://opcfoundation.org/UA/", "GeneratedBy"),
            new("AlwaysGeneratesEvent", "AlwaysGeneratesEvent", "http://opcfoundation.org/UA/", "AlwaysGeneratedBy"),
            new("HasNotifier", "HasNotifier", "http://opcfoundation.org/UA/", "NotifierOf"),
            new("HasEventSource", "HasEventSource", "http://opcfoundation.org/UA/", "EventSourceOf"),
            new("HasCondition", "HasCondition", "http://opcfoundation.org/UA/", "IsConditionOf"),
            new("HasOrderedComponent", "HasOrderedComponent", "http://opcfoundation.org/UA/", "OrderedComponentOf"),
            new("FromState", "FromState", "http://opcfoundation.org/UA/", "ToTransition"),
            new("ToState", "ToState", "http://opcfoundation.org/UA/", "FromTransition"),
            new("HasCause", "HasCause", "http://opcfoundation.org/UA/", "MayBeCausedBy"),
            new("HasEffect", "HasEffect", "http://opcfoundation.org/UA/", "MayBeAffectedBy"),
            new("HasGuard", "HasGuard", "http://opcfoundation.org/UA/", "GuardOf")
        ];

        public RelationshipTypesController(ADXDataService kusto)
        {
            _kusto = kusto;
            _kusto.Connect();
        }

        [HttpGet]
        public ActionResult<IEnumerable<I3xRelationshipType>> GetRelationshipTypes()
        {
            // No explicit relationship table in ADX – return well-known OPC UA reference types
            return Ok(KnownRelationshipTypes);
        }

        [HttpPost("query")]
        public ActionResult<IEnumerable<I3xRelationshipType>> QueryByElementId([FromBody] ElementIdQuery query)
        {
            var ids = new HashSet<string>(query.ElementIds);
            var results = KnownRelationshipTypes
                .Where(rt => ids.Contains(rt.ElementId))
                .ToList();

            return Ok(results);
        }
    }
}
