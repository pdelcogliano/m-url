using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M_url.Data.ResourceParameters;
using M_url.Data.Helpers;
using M_url.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace M_url.Data.Repositories
{
    public class MurlRepository : IMurlRepository, IDisposable
    {
        private MurlContext _murlContext;

        public MurlRepository(MurlContext murlContext)
        {
            _murlContext = murlContext ?? throw new ArgumentNullException(nameof(murlContext));
        }

        public async Task<PagedList<SlugEntity>> GetAllSlugsAsync(SlugsResourceParameters slugsResourceParameter)
        {
            if (slugsResourceParameter == null)
                throw new ArgumentNullException(nameof(slugsResourceParameter));

            IQueryable<SlugEntity> collection = _murlContext.Slugs;
            var orderedCollection = collection.OrderBy(c => c.Url);
            return await PagedList<SlugEntity>.Create(orderedCollection, slugsResourceParameter);
        }

        public async Task<IEnumerable<SlugEntity>> GetAllSlugsAsync(IEnumerable<string> slugs)
        {
            return await _murlContext.Slugs.Where(s => slugs.Contains(s.Slug)).ToListAsync();
        }

        public async Task<SlugEntity> GetSlugAsync(string slug)
        {
            return await _murlContext.Slugs.SingleOrDefaultAsync(u => u.Slug == slug);
        }

        public async Task<SlugEntity> GetByUrlAsync(string url)
        {
            return await _murlContext.Slugs.SingleOrDefaultAsync(u => u.Url == url);
        }

        public void AddSlug(SlugEntity newSlug)
        {
            if (newSlug == null)
                throw new ArgumentNullException(nameof(newSlug));

            _murlContext.Slugs.Add(newSlug);
        }

        public void DeleteSlug(SlugEntity slug)
        {
            _murlContext.Slugs.Remove(slug);
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return (await _murlContext.SaveChangesAsync() > 0);
            }
            catch(Exception e)
            {
                // todo: log error
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_murlContext != null)
                {
                    _murlContext.Dispose();
                    _murlContext = null;
                }
            }
        }
    }
}