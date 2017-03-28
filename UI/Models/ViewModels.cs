using System.Web.Mvc;

namespace UI.Models
{
        public class QAModel {
            
            public Question question { get; set; }
            public Answer answer { get; set; }

            //public QuestionComment questionComment { get; set; }
            //public AnswerComment answerComment { get; set; }
            //public int MarksBy { get; set; }
        }
}