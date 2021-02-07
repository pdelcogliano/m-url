using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace M_url.Models
{
    public class SlugDto
    {
        [MinLength(5, ErrorMessage = "{0} does not contain enough characters (Minimum 5)."),
            RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "This is not a valid {0}.")]
        public string Slug { get; set; }

        [Required(ErrorMessage = "A URL is required."),
            Url(ErrorMessage = "A valid URL must be provided.")]
        public string Url { get; set; }
    }
}
