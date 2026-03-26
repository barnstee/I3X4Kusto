using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public ActionResult<IEnumerable<I3xNamespace>> GetNamespaces() => new List<I3xNamespace> { new I3xNamespace("test1", "test2", "test3") };
    }
}
