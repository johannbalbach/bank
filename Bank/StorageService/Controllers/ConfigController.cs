using Microsoft.AspNetCore.Mvc;
using StorageService.Dto;
using StorageService.Services;

namespace StorageService.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigService _configService;

        public ConfigController(IConfigService config)
        {
            _configService = config;
        }

        [HttpPost]
        [Route("create/user/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateConfigResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> CreateConfig([FromBody] CreateConfigRequest request, [FromRoute] Guid id)
        {
            try
            {
                var response = await _configService.CreateConfig(request, id);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get/user/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetConfigResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> GetConfig([FromQuery] GetConfigRequest request, [FromRoute] Guid id)
        {
            try
            {
                var response = await _configService.GetConfig(request, id);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("delete/user/{id:guid}")]
        public async Task<IActionResult> DeleteConfig([FromQuery] DeleteConfigRequest request, [FromRoute] Guid id)
        {
            try
            {
                await _configService.RemoveConfig(request, id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
