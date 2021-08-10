using M_url.Api.ModelBinders;
using M_url.Data.Repositories;
using M_url.Domain.Entities;
using M_url.Models;
using M_url.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace M_url.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/slugcollections")]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [ApiController]
    public class SlugCollectionsController : ControllerBase
    {
        private readonly ILogger<SlugCollectionsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMurlRepository _murlRepository;

        public SlugCollectionsController(ILogger<SlugCollectionsController> logger, IConfiguration configuration, IMurlRepository murlRepository)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger)); //.CreateLogger();
            _configuration = configuration ??
                    throw new ArgumentNullException(nameof(configuration));
            _murlRepository = murlRepository ?? 
                throw new ArgumentNullException(nameof(murlRepository));
        }

        //  api.slugcollections/(slug1, slug2)
        /// <summary>
        /// Returns a collection of URLs
        /// </summary>
        /// <param name="slugs">list of slugs to retrieve</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     Get /api/slugscollection/(d25tRx, fN5jpz)
        ///
        /// </remarks>
        /// <returns>IEnumerable of slugs</returns>
        /// <response code="200">If all requested items are found</response>
        /// <response code="400">If slugs parameter is missing</response>
        /// <response code="404">If number of records found doesn't equal number of records requested</response>
        [HttpGet("({slugs})", Name = "GetSlugCollection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult> GetSlugCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<string> slugs)
        {
            if (slugs == null)
                return BadRequest();

            IEnumerable<SlugEntity> slugCollection = await _murlRepository.GetAllSlugsAsync(slugs);

            if (slugCollection.Count() == slugs.Count())
                return Ok(slugCollection);

            // return 404 if number of slugs to find doesn't equal number of slugs found.
            return NotFound();
        }

        /// <summary>
        /// Creates slugs in bulk
        /// </summary>
        /// <param name="slugsToAdd"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/slugcollections
        ///     [
        ///       {
        ///          "url": "http://www.site1.com"
        ///       },
        ///       {
        ///          "url": "http://www.site2.com"
        ///       }
        ///     ]
        /// </remarks>
        /// <returns>IEnumerable of newly created slugs</returns>
        /// <response code="201">If items were added successfully</response>
        /// <response code="400">If no values are provided in the slugsToAdd parameter</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult> CreateSlugCollection(IEnumerable<SlugForCreationDto> slugsToAdd)
        {
            _logger.LogInformation($"bulk add new slugs");

            if (slugsToAdd == null || ! slugsToAdd.Any())
                return BadRequest();
            
            List<SlugEntity> slugsToReturn = new();
            int slugLength = _configuration.GetValue<short>("SlugLength", 5);

            foreach (var slug in slugsToAdd)
            {
                if (string.IsNullOrWhiteSpace(slug.Slug))
                    slug.Slug = NanoidService.Create(slugLength);

                SlugEntity newSlug = new()
                {
                    Slug = slug.Slug,
                    Url = slug.Url
                };
                _murlRepository.AddSlug(newSlug);
                slugsToReturn.Add(newSlug);
            }
            
            await _murlRepository.SaveChangesAsync();
            string slugs = string.Join(",", slugsToAdd.Select(s => s.Slug));

            return CreatedAtRoute(nameof(GetSlugCollection), new { slugs }, slugsToAdd);
        }
    }
}