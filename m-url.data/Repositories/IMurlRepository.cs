using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using M_url.Data.ResourceParameters;
using M_url.Data.Helpers;
using M_url.Domain.Entities;

namespace M_url.Data.Repositories
{
    public interface IMurlRepository
    {
        Task<PagedList<SlugEntity>> GetAllSlugsAsync(SlugsResourceParameters slugsResourceParameters);

        Task<IEnumerable<SlugEntity>> GetAllSlugsAsync(IEnumerable<string> slugs);

        Task<SlugEntity> GetSlugAsync(string slug);

        Task<SlugEntity> GetByUrlAsync(string url);

        void AddSlug(SlugEntity newSlug);

        void DeleteSlug(SlugEntity slug);

        Task<bool> SaveChangesAsync();
    }
}