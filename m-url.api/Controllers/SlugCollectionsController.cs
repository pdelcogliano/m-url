using M_url.Api.ModelBinders;
using M_url.Data.Repositories;
using M_url.Domain.Entities;
using M_url.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using M_url.Services;

namespace M_url.Api.Controllers
{
    [ApiController]
    [Route("api/slugcollections")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
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
        /// <param name="slugs"></param>
        /// <returns></returns>
        [HttpGet("({slugs})", Name = "GetSlugCollection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateSlugCollection(IEnumerable<SlugForCreationDto> slugsToAdd)
        {
            _logger.LogInformation($"bulk add new slugs");

            if (slugsToAdd == null || slugsToAdd.Count() == 0)
                return BadRequest();
            
            List<SlugEntity> slugsToReturn = new List<SlugEntity>();
            int slugLength = _configuration.GetValue<short>("SlugLength", 5);

            foreach (var slug in slugsToAdd)
            {
                SlugEntity newSlug = new SlugEntity
                {
                    Slug = string.IsNullOrWhiteSpace(slug.Slug) ? NanoidService.Create(slugLength) : slug.Slug,
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