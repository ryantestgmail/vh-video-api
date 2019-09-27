using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using VideoApi.Common.Configuration;

namespace Video.API.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("beta")]
    [ApiController]
    public class BetaController : ControllerBase
    {
        private readonly IFeatureManager _featureManager;

        public BetaController(IFeatureManager featureManager)
        {
            _featureManager = featureManager;
        }
        /// <summary>
        /// Remove participants from a conference
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [AllowAnonymous]
        public IActionResult Beta()
        {
            var text = _featureManager.IsEnabled(nameof(FeatureFlags.Beta))
                ? "Welcome to the Beta"
                : "Welcome";
            return Ok(text);
        }
    }
}