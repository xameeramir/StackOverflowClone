using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class Marks
    {
        public int MarksId { get; set; }
        public int MarksAwarded { get; set; }
        public int MarksType { get; set; }
        public int postId { get; set; }
        public int MarksBy { get; set; }
        public string MarksByUname { get; set; }
        public DateTime AwardedDt { get; set; }
        public int postType { get; set; }

    }
}