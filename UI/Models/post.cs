using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Models
{
    public class post
    {
        public HandleErrorInfo error = null;

        public int postId { get; set; }
        public int postType { get; set; }
        public string postTitle { get; set; }
        public string postWiki { get; set; }
        public int postBy { get; set; }
        public DateTime postDate { get; set; }
        public int postEditBy { get; set; }
        public DateTime postEditDate { get; set; }
        public string postByUname { get; set; }
        public string postEditByUname { get; set; }
        public int postMarksEarned { get; set; }
        public int postPremiums { get; set; }
        public int postPremiumsBy { get; set; }
        public string postPremiumsByUname { get; set; }
        public int postQualityEarned { get; set; }
        public int postShares { get; set; }
        public string postStatus { get; set; }
        public int postStatusBy { get; set; }
        public string postStatusByUname { get; set; }
        public string postF1 { get; set; }
        public string postF2 { get; set; }
        public string postF3 { get; set; }
        public string postF4 { get; set; }
        public string postF5 { get; set; }
        public string postFlags { get; set; }
        public int postFlagsBy { get; set; }
        public string postFlagsByUname { get; set; }
    }
}