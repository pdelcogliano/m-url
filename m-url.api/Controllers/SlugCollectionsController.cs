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

namespace M_url.Api.Controllers
{
    [ApiController]
    [Route("api/slugcollections")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class SlugCollectionsController : ControllerBase
    {
        private readonly IMurlRepository _murlRepository;

        public SlugCollectionsController(IMurlRepository murlRepository)
        {
            _murlRepository = murlRepository ?? throw new ArgumentNullException(nameof(murlRepository));
        }

        //  api.slugcollections/(slug1, slug2)
        /// <summary>
        /// Returns a collection of URLs
        /// </summary>
        /// <param name="slugs"></param>
        /// <returns></returns>
        [HttpGet("({slugs})", Name = "GetSlugCollection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetSlugCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<string> slugs)
        {
            IEnumerable<SlugEntity> slugCollection = await _murlRepository.GetAllSlugsAsync(slugs);

            if (slugCollection.Count() != slugs.Count())
                return NotFound();

            return Ok(slugCollection);
        }

        /// <summary>
        /// Creates slugs in bulk
        /// </summary>
        /// <param name="slugsToAdd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateSlugCollection(IEnumerable<SlugForCreationDto> slugsToAdd)
        {
            List<SlugEntity> slugsToReturn = new List<SlugEntity>();

            foreach (var slug in slugsToAdd)
            {
                SlugEntity newSlug = new SlugEntity()
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
