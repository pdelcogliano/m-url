using M_url.Api.ModelBinders;
using M_url.Data.Repositories;
using M_url.Domain.Entities;
using M_url.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_url.Api.Controllers
{
    [ApiController]
    [Route("api/slugcollections")]
    public class SlugCollectionsController : ControllerBase
    {
        private readonly IMurlRepository _murlRepository;

        public SlugCollectionsController(IMurlRepository murlRepository)
        {
            _murlRepository = murlRepository ?? throw new ArgumentNullException(nameof(murlRepository));
        }

        //  api.slugcollections/(slug1, slug2)
        [HttpGet("({slugs})", Name = "GetSlugCollection")]
        public async Task<ActionResult> GetSlugCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<string> slugs)
        {
            IEnumerable<SlugEntity> slugCollection = await _murlRepository.GetAllSlugsAsync(slugs);

            if (slugCollection.Count() != slugs.Count())
                return NotFound();

            return Ok(slugCollection);
        }

        [HttpPost]
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
