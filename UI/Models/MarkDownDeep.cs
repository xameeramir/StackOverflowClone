using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Models
{
    public class MarkDownDeep
    {
        public string PostMarkdown { get { return ""; } set { } }

        public string PageUrl { get; set; }

        [Required(ErrorMessage = "Wiki is empty")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string MarkdownEditor { get; set; }
    }

}