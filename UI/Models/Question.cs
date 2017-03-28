using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public int AnswerCount { get; set; }
        public int CommentCount { get; set; }

        [Required(ErrorMessage = "* Your question")]
        [DisplayName("Your question")]
        public string Q_Title { get; set; }
        public string Ans_Title { get; set; }

        public string Q_WikiHtml { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "* Describe your question")]
        [DisplayName("Describe your question")]
        public string Q_Wiki { get; set; }

        /*                return @"

        Formatting template
        ## Example heading: ##

        **Example strong text**

        *example italic text*

        [example link][1]

        > Example quote

            Example code prefixed by 4 spaces

        `Example code surrounded with quotes`

         1. example numbered list item
         - example un-ordered list item

        ----------
        example horizontal rule above this text

          [1]: http://example.com
        ";
          */
        public int Q_By { get; set; }
        public DateTime Q_Dt { get; set; }
        public int Q_EditBy { get; set; }
        public DateTime Q_EditDt { get; set; }
        public int Q_Edits { get; set; }
        public int Q_MarksEarned { get; set; }
        public int Q_QualityEarned { get; set; }
        public int Q_Shares { get; set; }
        public int Q_Views { get; set; }

        [Required(ErrorMessage = "* Please enter upto 5 tags separated by comma")]
        [DisplayName("Tags (max 5, separated by comma)")]
        public string Q_Fields { get; set; }

        [Display(Name = "Related field")]
        public string Q_F1 { get; set; }

        [Display(Name = "Related field")]
        public string Q_F2 { get; set; }

        [Display(Name = "Related field")]
        public string Q_F3 { get; set; }

        [Display(Name = "Related field")]
        public string Q_F4 { get; set; }

        [Display(Name = "Related field")]
        public string Q_F5 { get; set; }

        public string Q_ByUname { get; set; }
        public string Q_EditByUname { get; set; }
        public int Q_Premiums { get; set; }
        public string Q_PremiumByUname { get; set; }
        public int Q_PremiumBy { get; set; }
        public int Q_Status { get; set; }
        public int Q_StatusBy { get; set; }
        public string Q_StatusByUname { get; set; }
        public string Q_Flags { get; set; }
        public int Q_FlagBy { get; set; }
        public string Q_FlagByUname { get; set; }

        [Display(Name = "Explain why edit was needed")]
        public string Q_EditSummary { get; set; }

        #region Helper properties

        [ScaffoldColumn(false)]
        public List<Question> questions { get; set; }

        [ScaffoldColumn(false)]
        public List<QuestionComment> questionComments { get; set; }

        [ScaffoldColumn(false)] //for accessing individual comments
        public QuestionComment questionComment { get; set; }

        [ScaffoldColumn(false)]
        public int postType { get { return 1; } set { } }

        public int Q_ByMarksScore { get; set; }

        #endregion

    }

}