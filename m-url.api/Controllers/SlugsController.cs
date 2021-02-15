using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M_url.Services;
using M_url.Models;
using M_url.Data.Repositories;
using M_url.Data.ResourceParameters;
using M_url.Domain.Entities;

namespace M_url.Api.Controllers
{
    [Route("api/slugs")]
    [ApiController]
    public class SlugsController : ControllerBase
    {
        private readonly ILogger<SlugsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMurlRepository _murlRepository;

        public SlugsController(ILogger<SlugsController> logger, IConfiguration configuration, IMurlRepository murlRepository)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger)); //.CreateLogger();
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
            _murlRepository =  murlRepository ??
                throw new ArgumentNullException(nameof(murlRepository));
        }

        // GET: api/murls>
        [HttpGet]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<SlugDto>>> GetSlugs([FromQuery] SlugsResourceParameters slugsResourceParameters)
        {
            _logger.LogInformation(string.Format($"getting all URL values "));

            // check if slug exists
            IEnumerable<SlugEntity> slugs = await _murlRepository.GetAllSlugsAsync(slugsResourceParameters);

            if (slugs == null)
                return new NotFoundObjectResult(new { message = "no URLs found." });
            else
                return new OkObjectResult(slugs);
        }

        // GET: api/slugs/{slug}>
        [HttpGet("{slug}", Name = "GetSlug")]
        public async Task<ActionResult<SlugDto>> GetSlug(string slug)
        {
            _logger.LogInformation(string.Format($"getting URL values for {slug}"));

            // check if slug exists
            SlugEntity existingSlug = await _murlRepository.GetSlugAsync(slug);

            if (existingSlug == null)
            {
                string fullUrl = string.Format($"{Request.GetEncodedUrl()}");
                return new NotFoundObjectResult(new { message = "URL not found.", body = new { slug, url = fullUrl } });
            }
            else
                return new OkObjectResult(existingSlug.Url);
        }

        // POST api/slugs
        [HttpPost]
        public async Task<ActionResult<SlugDto>> CreateSlug([FromBody] SlugForCreationDto slugForCreation)
        {
            _logger.LogInformation(string.Format($"creating new slug for URL: {slugForCreation.Url}"));

            // map to entity
            SlugEntity slugToAdd = new SlugEntity() { 
                Url = slugForCreation.Url 
            };
            
            int slugLength = _configuration.GetValue<short>("SlugLength", 5);
            slugToAdd.Slug = NanoidService.Create(slugLength);
            _murlRepository.AddSlug(slugToAdd);
            await _murlRepository.SaveChangesAsync();

            return CreatedAtRoute(nameof(GetSlug), new { slug = slugToAdd.Slug }, slugToAdd);
        }

        // Delete api/slugs/{slug}
        [HttpDelete("{slug}")]
        public async Task<ActionResult<SlugDto>> DeleteSlug(string slug)
        {
            SlugEntity slugToDelete = await _murlRepository.GetSlugAsync(slug);

            if (slugToDelete == null)
                return NotFound(slug);

            _murlRepository.DeleteSlug(slugToDelete);
            await _murlRepository.SaveChangesAsync();

            return Ok();
        }

        [HttpOptions]
        public IActionResult GetMurlOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }
    }
}