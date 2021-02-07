using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace M_url.Domain.Entities
{
    public class SlugEntity
    {
        [Key]
        public int Id { get; set; }

        [MinLength(5, ErrorMessage = "{0} does not contain enough characters (Minimum 5)."),
        RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "This is not a valid {0}.")]
        [MaxLength(10)]
        public string Slug { get; set; }

        [Required(ErrorMessage = "A URL is required."),
            Url(ErrorMessage = "A valid URL must be provided.")]
        public string Url { get; set; }
    }
}