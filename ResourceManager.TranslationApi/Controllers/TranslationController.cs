
using Microsoft.AspNetCore.Mvc;
using ResourceManager.TranslationApi.Models;
using ResourceManager.TranslationApi.Services;

namespace ResourceManager.TranslationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly TranslationService _translationService;

    public TranslationController(TranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost]
    public async Task<ActionResult<TranslationResponse>> Translate(
        [FromBody] TranslationRequest request)
    {
        try
        {
            var result = await _translationService.TranslateAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Failed to translate resources.",
                details = ex.Message
            });
        }
    }
}
