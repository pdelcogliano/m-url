using M_url.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace M_url.Data
{
    public class MurlContext : DbContext
    {
        public DbSet<SlugEntity> Slugs { get; set; }

        public MurlContext(DbContextOptions<MurlContext> options)
          : base(options)
        {

        }
    }
}
