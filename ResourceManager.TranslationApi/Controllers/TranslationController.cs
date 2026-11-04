
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
            if (!Request.Headers.TryGetValue("x-goog-api-key", out var apiKey) ||
            string.IsNullOrWhiteSpace(apiKey))
            {
                return BadRequest(new
                {
                    error = "Gemini API key is missing."
                });
            }

            var result = await _translationService.TranslateAsync(
                request,
                apiKey.ToString());

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
