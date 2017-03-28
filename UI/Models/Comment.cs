using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string CommentTxt { get; set; }

        public string CommentTxtHtml { get; set; }

        public int CommentBy { get; set; }

        public string CommentByUname { get; set; }

        public DateTime CommentDt { get; set; }

        public int CommentStatus { get; set; }

        public int CommentStatusBy { get; set; }

        public int CommentMarksEarned { get; set; }

        public int CommentByMarksScore { get; set; }
        public int CommentEdits { get; set; }

        /// <summary>
        /// 1: question comment, 2: answer comment
        /// </summary>
        public int CommentType { get; set; }
    }

    public class QuestionComment : Comment
    {
        [ScaffoldColumn(false)]
        public List<QuestionComment> QuestionComments { get; set; }

        [ScaffoldColumn(false)]
        public int postType { get { return 101; } set { } }

        public int QuestionId { get; set; }

        public static QuestionComment InitializeIfNone(QuestionComment questionComment)
        {
            if (questionComment == null)
            {
                questionComment = new UI.Models.QuestionComment();
                questionComment.QuestionComments = new List<UI.Models.QuestionComment>();
            }
            return questionComment;
        }

        public List<QuestionComment> GenModel4mDS(DataSet dsQuestionComments)
        {

            QuestionComment model = new QuestionComment();
            model.QuestionComments = new List<QuestionComment>();

            if (dsQuestionComments.RowsExists())
            {
                DataTable dtQuestionComments = dsQuestionComments.Tables[0];

                for (int i = 0; i < dtQuestionComments.Rows.Count; i++)
                {
                    QuestionComment item = new QuestionComment();
                    item.CommentBy = int.Parse(dtQuestionComments.Rows[i]["CommentBy"].ToString());
                    item.CommentByUname = dtQuestionComments.Rows[i]["CommentByUname"].ToString();
                    item.CommentByMarksScore = int.Parse(dtQuestionComments.Rows[i]["CommentByMarksScore"].ToString());
                    item.CommentDt = DateTime.Parse(dtQuestionComments.Rows[i]["CommentDt"].ToString());
                    item.CommentEdits = int.Parse(dtQuestionComments.Rows[i]["CommentEdits"].ToString());
                    item.CommentId = int.Parse(dtQuestionComments.Rows[i]["CommentId"].ToString());
                    if (dtQuestionComments.Rows[i]["CommentMarksEarned"].ToString() != "")
                    {
                        item.CommentMarksEarned = int.Parse(dtQuestionComments.Rows[i]["CommentMarksEarned"].ToString());
                    }
                    item.CommentStatus = int.Parse(dtQuestionComments.Rows[i]["CommentStatus"].ToString());
                    item.CommentStatusBy = int.Parse(dtQuestionComments.Rows[i]["CommentStatusBy"].ToString());
                    item.CommentTxt = dtQuestionComments.Rows[i]["CommentTxt"].ToString().SanitizeOutput();
                    item.CommentTxtHtml = dtQuestionComments.Rows[i]["CommentTxtHtml"].ToString().SanitizeOutput();
                    item.QuestionId = int.Parse(dtQuestionComments.Rows[i]["QuestionId"].ToString());
                    model.QuestionComments.Add(item);
                }
            }
            return model.QuestionComments;
        }

    }

    public class AnswerComment : Comment
    {
        [ScaffoldColumn(false)]
        public List<AnswerComment> AnswerComments { get; set; }

        [ScaffoldColumn(false)]
        public int postType { get { return 102; } set { } }

        public int AnswerId { get; set; }
        public int QuestionId { get; set; }

        public static AnswerComment InitializeIfNone(AnswerComment answerComment)
        {
            if (answerComment == null)
            {
                answerComment = new UI.Models.AnswerComment();
                answerComment.AnswerComments = new List<UI.Models.AnswerComment>();
            }
            return answerComment;
        }

        public List<AnswerComment> GenModel4mDS(DataSet dsAnswerComments)
        {

            AnswerComment model = new AnswerComment();
            model.AnswerComments = new List<AnswerComment>();

            if (dsAnswerComments.RowsExists())
            {
                DataTable dtAnswerComments = dsAnswerComments.Tables[0];

                for (int i = 0; i < dtAnswerComments.Rows.Count; i++)
                {
                    AnswerComment item = new AnswerComment();
                    item.CommentBy = int.Parse(dtAnswerComments.Rows[i]["CommentBy"].ToString());
                    item.CommentByUname = dtAnswerComments.Rows[i]["CommentByUname"].ToString();
                    item.CommentByMarksScore = int.Parse(dtAnswerComments.Rows[i]["CommentByMarksScore"].ToString());
                    item.CommentDt = DateTime.Parse(dtAnswerComments.Rows[i]["CommentDt"].ToString());
                    item.CommentEdits = int.Parse(dtAnswerComments.Rows[i]["CommentEdits"].ToString());
                    item.CommentId = int.Parse(dtAnswerComments.Rows[i]["CommentId"].ToString());
                    if (dtAnswerComments.Rows[i]["CommentMarksEarned"].ToString() != "")
                    {
                        item.CommentMarksEarned = int.Parse(dtAnswerComments.Rows[i]["CommentMarksEarned"].ToString());
                    }
                    item.CommentStatus = int.Parse(dtAnswerComments.Rows[i]["CommentStatus"].ToString());
                    item.CommentStatusBy = int.Parse(dtAnswerComments.Rows[i]["CommentStatusBy"].ToString());
                    item.CommentTxt = dtAnswerComments.Rows[i]["CommentTxt"].ToString().SanitizeOutput();
                    item.AnswerId = int.Parse(dtAnswerComments.Rows[i]["AnswerId"].ToString());
                    model.AnswerComments.Add(item);
                }
            }
            return model.AnswerComments;
        }

    }
}