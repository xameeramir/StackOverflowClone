using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Services;
using MySql.Data.MySqlClient;
using UI.Models;

namespace UI
{
    /// <summary>
    /// Summary description for AnswerWS
    /// </summary>
    [WebService(Namespace = "http://knowrdible.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class AnswerWS : System.Web.Services.WebService
    {

        DBUtil dbUtil = null;

        [WebMethod]
        public Answer AnswersByUser(int A_By)
        {
            Answer model = new Answer();
            MySqlCommand cmd = new MySqlCommand("spGetAnswerByAnswererID");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pA_By", A_By);
            dbUtil = new DBUtil(3);

            DataSet dsAnswersByUser = dbUtil.FillDataSet(cmd);
            model = GenModel4mDS(dsAnswersByUser);
            return model;

        }


        [WebMethod]
        public DataSet GetAnswersForQuestion(string procName, int QuestionId)
        {
            DataSet Answers_ds = new DataSet();
            if (procName == null)
            {
                procName = "spGetTopAnswersByQ_Id";
            }

            //connect to questions database
            dbUtil = new DBUtil(3);

            MySqlCommand cmd = new MySqlCommand(procName);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQuestionId", QuestionId);
            Answers_ds = dbUtil.FillDataSet(cmd);

            return Answers_ds;
        }

        public Answer GenModel4mDS(DataSet Answers_ds)
        {
            dbUtil = new DBUtil(3);
            Answer model = new Answer();
            model.answers = new List<Answer>();

            if (Answers_ds.RowsExists())
            {
                DataTable All_Answers_dt = Answers_ds.Tables[0];

                for (int i = 0; i < All_Answers_dt.Rows.Count; i++)
                {
                    if (All_Answers_dt.Rows[0]["QuestionId"].ToString() != "")
                    {
                        Answer item = new Answer();
                        item.QuestionId = int.Parse(All_Answers_dt.Rows[i]["QuestionId"].ToString());
                        item.AnswerId = int.Parse(All_Answers_dt.Rows[i]["AnswerId"].ToString());
                        item.CommentCount = int.Parse(All_Answers_dt.Rows[i]["CommentCount"].ToString());
                        item.A_Title = All_Answers_dt.Rows[i]["A_Title"].ToString().SanitizeOutput();
                        item.A_Wiki = All_Answers_dt.Rows[i]["A_Wiki"].ToString().SanitizeOutput();
                        item.A_WikiHtml = All_Answers_dt.Rows[i]["A_WikiHtml"].ToString();
                        item.A_By = int.Parse(All_Answers_dt.Rows[i]["A_By"].ToString());
                        item.A_Dt = Convert.ToDateTime(All_Answers_dt.Rows[i]["A_Dt"].ToString());
                        item.A_EditBy = int.Parse(All_Answers_dt.Rows[i]["A_EditBy"].ToString());
                        item.A_Views = int.Parse(All_Answers_dt.Rows[i]["A_Views"].ToString());
                        item.A_EditDt = Convert.ToDateTime(All_Answers_dt.Rows[i]["A_EditDt"].ToString());
                        item.A_Edits = int.Parse(All_Answers_dt.Rows[i]["A_Edits"].ToString());
                        item.A_ByUname = All_Answers_dt.Rows[i]["A_ByUname"].ToString();
                        if (All_Answers_dt.Rows[i]["A_MarksEarned"].ToString() != "")
                        {
                            item.A_MarksEarned = int.Parse(All_Answers_dt.Rows[i]["A_MarksEarned"].ToString());
                        }
                        item.A_PremiumsEarned = int.Parse(All_Answers_dt.Rows[i]["A_PremiumsEarned"].ToString());
                        item.A_Status = int.Parse(All_Answers_dt.Rows[i]["A_Status"].ToString());
                        item.A_StatusBy = int.Parse(All_Answers_dt.Rows[i]["A_StatusBy"].ToString());
                        item.A_Flags = int.Parse(All_Answers_dt.Rows[i]["A_Flags"].ToString());
                        item.A_FlagBy = int.Parse(All_Answers_dt.Rows[i]["A_FlagBy"].ToString());
                        item.A_QualityEarned = int.Parse(All_Answers_dt.Rows[i]["A_QualityEarned"].ToString());
                        item.A_ByMarksScore = int.Parse(All_Answers_dt.Rows[i]["A_ByMarksScore"].ToString());
                        if (All_Answers_dt.Rows[i]["A_EditSummary"].ToString() != "")
                        {
                            item.A_EditSummary = All_Answers_dt.Rows[i]["A_EditSummary"].ToString();
                        }
                        item.answerComments = new List<AnswerComment>();

                        if (item.CommentCount > 0)
                        {
                            item.answerComment = new AnswerComment(); //needed for model generation
                            MySqlCommand cmd = new MySqlCommand("spGetAnswerComments");
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("pAnswerId", item.AnswerId);
                            DataSet dsAnswerComments = dbUtil.FillDataSet(cmd);
                            item.answerComments = item.answerComment.GenModel4mDS(dsAnswerComments);
                            //TODO: pass comments for the answer to the view
                        }

                        model.answers.Add(item);

                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Single reusable function for create/adding and updating answers
        /// </summary>
        /// <param name="model">Answer to be updated</param>
        /// <param name="WriteType"> 1: Create, 2: Update </param>
        /// <returns> Status of write </returns>
        public string WriteAnswer(Answer model, int WriteType, Controller _Controller)
        {

            var UserNumber = _Controller.User.Identity.GetCurUserNumber();

            DBUtil objDBUtil = new DBUtil(3);
            MySqlCommand cmd = null;

            model.A_WikiHtml = model.A_Wiki.ConvertMdToHtml();

            if (WriteType == 1)
            {
                cmd = new MySqlCommand("spAddAnswer");
                cmd.Parameters.AddWithValue("pA_By", UserNumber);
                cmd.Parameters.AddWithValue("pQuestionId",model.QuestionId);
            }
            else if (WriteType == 2)
            {
                cmd = new MySqlCommand("spUpdateAnswer");
                cmd.Parameters.AddWithValue("pAnswerId", model.AnswerId);
                cmd.Parameters.AddWithValue("pA_EditBy", UserNumber);
                cmd.Parameters.AddWithValue("pA_EditSummary", model.A_EditSummary);
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pA_Title", model.A_Title.SanitizeInput());
            cmd.Parameters.AddWithValue("pA_Wiki", model.A_Wiki.SanitizeInput());
            cmd.Parameters.AddWithValue("pA_WikiHtml", model.A_WikiHtml);
            
            DataSet AddAnswer = objDBUtil.FillDataSet(cmd);
            if (AddAnswer.RowsExists())
            {
                model.answers = new List<Answer>();
                model.answers.Add(model);
                model.A_Dt = DateTime.Now; //to avoid number of day(s) difference
                int resTbl = AddAnswer.Tables.Count - 1;
                if (int.Parse(AddAnswer.Tables[resTbl].Rows[0]["StatusCode"].ToString()) != 0)
                {
                    return AddAnswer.Tables[resTbl].Rows[0]["StatusMsg"].ToString();
                }
            }

            return null;
        }

        public Answer GetAnswersForQuestion(int QuestionId)
        {
            DBUtil objDBUtil = new DBUtil(3);
            MySqlCommand cmd = new MySqlCommand("spGetAllAnswerByQ_Id");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQuestionId", QuestionId);
            DataSet AnswersForQuestion = objDBUtil.FillDataSet(cmd);

            return GenModel4mDS(Answers_ds: AnswersForQuestion).answers[0];
        }

        public Answer GetAnswerById(int AnswerId, string VisitorIP)
        {
            DBUtil objDBUtil = new DBUtil(3);
            MySqlCommand cmd = new MySqlCommand("spGetAnswerById");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pAnswerId", AnswerId);
            cmd.Parameters.AddWithValue("pVisitorIP", VisitorIP);
            DataSet AnswerById = objDBUtil.FillDataSet(cmd);

            return GenModel4mDS(Answers_ds: AnswerById).answers[0]; //it returns one single answer
        }


    }
}
