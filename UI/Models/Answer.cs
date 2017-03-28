using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class Answer
    {

        public int AnswerId { get; set; }
        
        public int QuestionId { get; set; }
        public int CommentCount { get; set; }
        public int A_Views { get; set; }

        [Required(ErrorMessage = "* Your answer")]
        [DisplayName("Your answer")]
        public string A_Title { get; set; }

        public string A_WikiHtml { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "* Describe your answer")]
        [DisplayName("Describe your answer")]
        public string A_Wiki { get; set; }
        
        public int A_By { get; set; }
        public string A_ByUname { get; set; }

        public DateTime A_Dt { get; set; }
        public int A_EditBy { get; set; }
        public DateTime A_EditDt { get; set; }
        public int A_Edits { get; set; }
        public int A_MarksEarned { get; set; }
        public int A_PremiumsEarned { get; set; }
        public int A_Status { get; set; }
        public int A_StatusBy { get; set; }
        public int A_Flags { get; set; }
        public int A_FlagBy { get; set; }
        public int A_QualityEarned { get; set; }

        [Display(Name = "Explain why edit was needed")]
        public string A_EditSummary { get; set; }

        #region Helper

        [ScaffoldColumn(false)]
        public int A_ByMarksScore { get; set; }

        [ScaffoldColumn(false)]
        public List<Answer> answers { get; set; }

        [ScaffoldColumn(false)]
        public List<AnswerComment> answerComments { get; set; }

        [ScaffoldColumn(false)] //for accessing individual comments
        public AnswerComment answerComment { get; set; }

        [ScaffoldColumn(false)]
        public int postType { get { return 2; } set { } }

        #endregion
    
        public static Answer InitializeIfNone(Answer answer)
        {
            if (answer == null)
            {
                answer = new UI.Models.Answer();
                answer.answers = new List<UI.Models.Answer>();
            }
            if (answer.answerComments == null)
            {
                answer.answerComments = new List<AnswerComment>();
            }

            return answer;
        }

    }
}