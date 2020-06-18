namespace TestOkur.Identity.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel.DataAnnotations;
    using TestOkur.Identity.Configuration;
    using TestOkur.Infrastructure.Mvc.Diagnostic;

    [Route("api/diagnostic")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly AppConfiguration _applicationConfiguration;

        public DiagnosticController(AppConfiguration applicationConfiguration)
        {
            _applicationConfiguration = applicationConfiguration;
        }

        public IActionResult Get([FromQuery, Required] string key)
        {
            return _applicationConfiguration.Key != key
                ? (IActionResult)Unauthorized()
                : Content(DiagnosticReport.Generate().ToString());
        }
    }
}