using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using MySql.Data.MySqlClient;
using UI.Models;

namespace UI
{
    /// <summary>
    /// Summary description for QuestionWS
    /// </summary>
    [WebService(Namespace = "http://knowrdible.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class QuestionWS : System.Web.Services.WebService
    {
        DBUtil dbUtil = new DBUtil(3);

        public Question GenModel4mDS(DataSet Questions_ds)
        {
            Question model = new Question();
            if (Questions_ds.RowsExists())
            {
                DataTable All_Questions_dt = Questions_ds.Tables[0];
                model.questions = new List<Question>();

                for (int i = 0; i < All_Questions_dt.Rows.Count; i++)
                {
                    if (All_Questions_dt.Rows[i]["Q_Title"].ToString() != "")
                    {
 
                        Question item = new Question();
                        item.QuestionId = int.Parse(All_Questions_dt.Rows[i]["QuestionId"].ToString());
                        item.CommentCount = int.Parse(All_Questions_dt.Rows[i]["CommentCount"].ToString());
                        item.AnswerCount = int.Parse(All_Questions_dt.Rows[i]["AnswerCount"].ToString());
                        item.Q_Title = All_Questions_dt.Rows[i]["Q_Title"].ToString().SanitizeOutput();
                        item.Ans_Title = All_Questions_dt.Rows[i]["Ans_Title"].ToString().SanitizeOutput();
                        item.Q_Wiki = All_Questions_dt.Rows[i]["Q_Wiki"].ToString().SanitizeOutput();
                        item.Q_WikiHtml = All_Questions_dt.Rows[i]["Q_WikiHtml"].ToString().SanitizeOutput();
                        item.Q_By = int.Parse(All_Questions_dt.Rows[i]["Q_By"].ToString());
                        item.Q_Dt = Convert.ToDateTime(All_Questions_dt.Rows[i]["Q_Dt"].ToString());
                        item.Q_EditBy = int.Parse(All_Questions_dt.Rows[i]["Q_EditBy"].ToString());
                        item.Q_EditDt = Convert.ToDateTime(All_Questions_dt.Rows[i]["Q_EditDt"].ToString());
                        item.Q_Edits = int.Parse(All_Questions_dt.Rows[i]["Q_Edits"].ToString());
                        item.Q_QualityEarned = int.Parse(All_Questions_dt.Rows[i]["Q_QualityEarned"].ToString());
                        item.Q_Shares = int.Parse(All_Questions_dt.Rows[i]["Q_Shares"].ToString());
                        item.Q_Views = int.Parse(All_Questions_dt.Rows[i]["Q_Views"].ToString());
                        item.Q_Fields = All_Questions_dt.Rows[i]["Q_Fields"].ToString();
                        item.Q_F1 = All_Questions_dt.Rows[i]["Q_F1"].ToString();
                        item.Q_F2 = All_Questions_dt.Rows[i]["Q_F2"].ToString();
                        item.Q_F3 = All_Questions_dt.Rows[i]["Q_F3"].ToString();
                        item.Q_F4 = All_Questions_dt.Rows[i]["Q_F4"].ToString();
                        item.Q_F5 = All_Questions_dt.Rows[i]["Q_F5"].ToString();
                        if (All_Questions_dt.Rows[i]["Q_MarksEarned"].ToString() != null && All_Questions_dt.Rows[i]["Q_MarksEarned"].ToString() != "")
                        {
                            item.Q_MarksEarned = int.Parse(All_Questions_dt.Rows[i]["Q_MarksEarned"].ToString());
                        }
                        item.Q_ByUname = All_Questions_dt.Rows[i]["Q_ByUname"].ToString();
                        item.Q_EditByUname = All_Questions_dt.Rows[i]["Q_EditByUname"].ToString();
                        item.Q_Premiums = int.Parse(All_Questions_dt.Rows[i]["Q_Premiums"].ToString());
                        item.Q_PremiumByUname = All_Questions_dt.Rows[i]["Q_PremiumByUname"].ToString();
                        item.Q_PremiumBy = int.Parse(All_Questions_dt.Rows[i]["Q_PremiumBy"].ToString());
                        item.Q_Status = int.Parse(All_Questions_dt.Rows[i]["Q_Status"].ToString());
                        item.Q_StatusBy = int.Parse(All_Questions_dt.Rows[i]["Q_StatusBy"].ToString());
                        item.Q_StatusByUname = All_Questions_dt.Rows[i]["Q_StatusByUname"].ToString();
                        item.Q_Flags = All_Questions_dt.Rows[i]["Q_Flags"].ToString();
                        item.Q_FlagBy = int.Parse(All_Questions_dt.Rows[i]["Q_FlagBy"].ToString());
                        item.Q_FlagByUname = All_Questions_dt.Rows[i]["Q_FlagByUname"].ToString();
                        if(All_Questions_dt.Rows[i]["Q_ByMarksScore"].ToString() != "")
                        {
                            item.Q_ByMarksScore = int.Parse(All_Questions_dt.Rows[i]["Q_ByMarksScore"].ToString());
                        }
                        if (All_Questions_dt.Rows[i]["Q_EditSummary"].ToString() != "")
                        {
                            item.Q_EditSummary = All_Questions_dt.Rows[i]["Q_EditSummary"].ToString();
                        }

                        item.questionComments = new List<QuestionComment>();

                        if (item.CommentCount > 0)
                        {
                            item.questionComment = new QuestionComment(); //needed for model generation

                            MySqlCommand cmd = new MySqlCommand("spGetQuestionComments");
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("pQuestionId", item.QuestionId);
                            DataSet dsQuestionComments = dbUtil.FillDataSet(cmd);
                            item.questionComments = item.questionComment.GenModel4mDS(dsQuestionComments);
                            //TODO: pass comments for the question to the view
                        }

                        model.questions.Add(item);

                    }
                }
            }

            return model;

        }

        [WebMethod]
        public Question QuestionsByUser(int Q_By)
        {
            Question model = new Question();
            MySqlCommand cmd = new MySqlCommand("spGetQuestionsByAskerID");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQ_By", Q_By);
            dbUtil = new DBUtil(3);

            DataSet dsQuestionsByUser = dbUtil.FillDataSet(cmd);
            QuestionWS Qws = new QuestionWS();
            model = Qws.GenModel4mDS(dsQuestionsByUser);
            return model;

        }

        public Question GetQuestionsBySortFilter(string Sort)
        {
            string procName;

            switch (Sort)
            {
                case "Premium":
                    procName = "spGetPremiumQuestions";
                    break;

                case "Trending":
                    procName = "spGetTrendingQuestions";
                    break;

                case "Unanswered":
                    procName = "spGetUnansweredQuestions";
                    break;

                case "Live":
                    procName = "spGetLiveQuestions";
                    break;

                case "Popular":
                    procName = "spGetPopularQuestions";
                    break;

                case "Oldest":
                    procName = "spGetOldestQuestions";
                    break;

                case "Newest":
                default:
                    procName = "spGetAllQuestions";
                    break;
            }

            QuestionWS Qws = new QuestionWS();
            Question model = Qws.GenModel4mDS(
                Questions_ds: Qws.GetQuestions(procName));

            return model;

        }

        //
        // GET: /Question/GetQuestions
        [WebMethod]
        public DataSet GetQuestions(string procName)
        {
            DataSet Questions_ds = new DataSet();

            //connect to questions database
            DBUtil objDBUtil = new DBUtil(3);

            MySqlCommand cmd = new MySqlCommand(procName);
            cmd.CommandType = CommandType.StoredProcedure;
            Questions_ds = objDBUtil.FillDataSet(cmd);

            return Questions_ds;
        }

        public Question GetQuestionById(int QuestionId, string VisitorIP)
        {

            DBUtil objDBUtil = new DBUtil(3);
            DataSet QuestionByID = new DataSet();
            MySqlCommand cmd = new MySqlCommand("spGetQuestionByID");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQuestionId", QuestionId);
            cmd.Parameters.AddWithValue("pVisitorIP", VisitorIP);
            QuestionByID = objDBUtil.FillDataSet(cmd);

            return GenModel4mDS(Questions_ds: QuestionByID).questions[0];
        }

        /// <summary>
        /// Single reusable function for create/adding and updating questions
        /// </summary>
        /// <param name="model">Question to be updated</param>
        /// <param name="WriteType"> 1: Create, 2: Update </param>
        /// <returns> Status of write </returns>
        public DataSet WriteQuestion(Question model, int WriteType, Controller _Controller)
        {
            var UserNumber = _Controller.User.Identity.GetCurUserNumber();

            DBUtil objDBUtil = new DBUtil(3);
            MySqlCommand cmd = null;

            #region Assign field values

            string[] Q_FieldsArr = model.Q_Fields.SanitizeInput().Split(',');
            if (Q_FieldsArr.Length > 0)
            {
                model.Q_F1 = Q_FieldsArr[0];
            }
            else { model.Q_F1 = "none"; }

            if (Q_FieldsArr.Length > 1)
            {
                model.Q_F2 = Q_FieldsArr[1];
            }
            else { model.Q_F2 = "none"; }

            if (Q_FieldsArr.Length > 2)
            {
                model.Q_F3 = Q_FieldsArr[2];
            }
            else { model.Q_F3 = "none"; }

            if (Q_FieldsArr.Length > 3)
            {
                model.Q_F4 = Q_FieldsArr[3];
            }
            else { model.Q_F4 = "none"; }

            if (Q_FieldsArr.Length > 4)
            {
                model.Q_F5 = Q_FieldsArr[4];
            }
            else { model.Q_F5 = "none"; }

            #endregion

            model.Q_WikiHtml = model.Q_Wiki.ConvertMdToHtml();

            if (WriteType == 1)
            {
                cmd = new MySqlCommand("spAddQuestion");
                cmd.Parameters.AddWithValue("pQ_By", UserNumber);
            }
            else if (WriteType == 2)
            {
                cmd = new MySqlCommand("spUpdateQuestion");
                cmd.Parameters.AddWithValue("pQuestionId", model.QuestionId);
                cmd.Parameters.AddWithValue("pQ_EditBy", UserNumber);
                cmd.Parameters.AddWithValue("pQ_EditSummary", model.Q_EditSummary);
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQ_Title", model.Q_Title.SanitizeInput());
            cmd.Parameters.AddWithValue("pQ_Wiki", model.Q_Wiki.SanitizeInput());
            cmd.Parameters.AddWithValue("pQ_WikiHtml", model.Q_WikiHtml);
            cmd.Parameters.AddWithValue("pQ_Fields", model.Q_Fields);
            cmd.Parameters.AddWithValue("pQ_F1", model.Q_F1);
            cmd.Parameters.AddWithValue("pQ_F2", model.Q_F2);
            cmd.Parameters.AddWithValue("pQ_F3", model.Q_F3);
            cmd.Parameters.AddWithValue("pQ_F4", model.Q_F4);
            cmd.Parameters.AddWithValue("pQ_F5", model.Q_F5);
            DataSet AddQuestion = objDBUtil.FillDataSet(cmd);

            return AddQuestion;
        }

    }
}