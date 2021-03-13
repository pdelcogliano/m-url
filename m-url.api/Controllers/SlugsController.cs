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
using M_url.Data.Helpers;
using M_url.Data.Repositories;
using M_url.Data.ResourceParameters;
using M_url.Domain.Entities;
using System.Text.Json;
using System.Dynamic;
using System.Net.Mime;

namespace M_url.Api.Controllers
{
    [Route("api/slugs")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
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
            _murlRepository = murlRepository ??
                throw new ArgumentNullException(nameof(murlRepository));
        }

        // GET: api/murls>
        /// <summary>
        /// Returns a URL for the given parameters
        /// </summary>
        /// <param name="slugsResourceParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetSlugs")]
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SlugDto>>> GetSlugs([FromQuery] SlugsResourceParameters slugsResourceParameters)
        {
            _logger.LogInformation(string.Format($"GetSlugs [HTTP Get]: getting all URL values for page number: {slugsResourceParameters.PageNumber} and page size: {slugsResourceParameters.PageSize}"));

            // check if slug exists
            PagedList<SlugEntity> slugs = await _murlRepository.GetAllSlugsAsync(slugsResourceParameters);
            var paginationMetaData = CreatePaginationMetaData(slugs, slugsResourceParameters);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

            if (slugs == null || slugs.Count() == 0)
                return new NotFoundObjectResult(new { message = "no URLs found." });
            else
                return new OkObjectResult(slugs);
        }

        // GET: api/slugs/{slug}>
        /// <summary>
        /// Returns a URL for a given slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet("{slug}", Name = "GetSlug")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <summary>
        /// Creates a slug
        /// </summary>
        /// <param name="slugForCreation"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SlugDto>> CreateSlug([FromBody] SlugForCreationDto slugForCreation)
        {
            _logger.LogInformation(string.Format($"creating new slug for URL: {slugForCreation.Url}"));

            // map to entity
            SlugEntity slugToAdd = new SlugEntity
            { 
                Url = slugForCreation.Url 
            };
            
            int slugLength = _configuration.GetValue<short>("SlugLength", 5);
            slugToAdd.Slug = NanoidService.Create(slugLength);
            _murlRepository.AddSlug(slugToAdd);
            await _murlRepository.SaveChangesAsync();

            return CreatedAtRoute(nameof(GetSlug), new { slug = slugToAdd.Slug }, slugToAdd);
        }

        // Delete api/slugs/{slug}
        /// <summary>
        /// Deletes a slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpDelete("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        private string CreateSlugsResourceUri(SlugsResourceParameters slugsResourceParameters, ResourceUriType type)
        {
            dynamic routevalues = new ExpandoObject();
            routevalues.pageNumber = slugsResourceParameters.PageNumber;
            routevalues.pageSize = slugsResourceParameters.PageSize;
            routevalues.mainCategory = slugsResourceParameters.MainCategory;
            routevalues.searchQuery = slugsResourceParameters.SearchQuery;

            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    routevalues.pageNumber = slugsResourceParameters.PageNumber - 1;
                    break;

                case ResourceUriType.NextPage:
                    routevalues.pageNumber = slugsResourceParameters.PageNumber + 1;
                    break;

                default:
                    break;
            }
            return Url.Link("GetSlugs", routevalues);
        }

        private object CreatePaginationMetaData(PagedList<SlugEntity> slugs, SlugsResourceParameters slugsResourceParameters)
        {
            string previousPageLink = slugs.HasPrevious ? CreateSlugsResourceUri(slugsResourceParameters, ResourceUriType.PreviousPage) : null;
            string nextPageLink = slugs.HasNext ? CreateSlugsResourceUri(slugsResourceParameters, ResourceUriType.NextPage) : null;

            return new
            {
                totalCount = slugs.TotalCount,
                pageSize = slugs.PageSize,
                currentPage = slugs.CurrentPage,
                totalPages = slugs.TotalPages,
                previousPageLink,
                nextPageLink
            };
        }
    }
}