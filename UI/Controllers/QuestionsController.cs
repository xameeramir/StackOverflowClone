using UI.Models;
using System;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace UI.Controllers
{
    public class QuestionsController : Controller
    {
        private QuestionDbContext db = new QuestionDbContext();
        DBUtil dbUtil = null;

        public ActionResult Guidelines()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult QuestionsByUser(int Q_By, string Q_ByUName)
        {
            QAModel model = new QAModel();

            QuestionWS Qws = new QuestionWS();
            model.question = Qws.QuestionsByUser(Q_By);

            TempData["Title"] = string.Format("Questions asked by {0}", Q_ByUName);

            return View("Index", model);
        }

        // GET: Questions
        [AllowAnonymous]
        //[ExportModelStateToTempData]
        public ActionResult Index(QAModel model, string Sort = "")
        {
            try
            {
                if (model.question == null)
                {
                    model.question = new Question();
                }

                if (model.question.questions == null)
                {
                    QuestionWS Qws = new QuestionWS();
                    model.question = Qws.GetQuestionsBySortFilter(Sort);
                }

                if (Sort != null)
                {
                    TempData["Title"] = Sort + " questions";
                }

                if (model.question.questions.Count > 0)
                {
                    return View(model);
                }
            }
            catch (Exception)
            {
                //TODO: log this error
            }

            TempData["ErrorPrevention"] = "Do you want to <a href='/Questions/Create'> ask a question </a>?";
            ModelState.AddModelError("", "An error occured while retrieving questions");
            return View("Error");
        }

        // GET: Questions/Details/5
        /// <summary>
        /// Single reusable action for displaying question
        /// </summary>
        /// <param name="model"> The question answer to display (with answer) </param>
        /// <param name="_View"> Eg: Details for display, Edit for editing, Delete for deleting </param>
        /// <param name="question"> The question to display </param>
        /// <returns> The view specified by _View </returns>
        //[ImportModelStateFromTempData]
        //[PageView]
            public ActionResult Details(int QuestionId, string _View)
        {
            QAModel model = new QAModel();
            QuestionComment questionComment = new QuestionComment();
            AnswerComment answerComment = new AnswerComment();
            dbUtil = new DBUtil(3);
            if (_View == "Edit")
            {
                if (!Request.IsAuthenticated)
                {
                    TempData["RedirectMsg"] = "You must be logged in first";
                    return RedirectToActionPermanent("Login", "Account");
                }
            }

                if (QuestionId > 0)
                {
                    model.question = new Question();

                    QuestionWS Qws = new QuestionWS();
                    model.question = Qws.GetQuestionById(QuestionId, this.GetVisitorIP(Session["VisitorIP"]));

                    if (model.question.AnswerCount > 0)
                    {
                        AnswerWS Aws = new AnswerWS();
                        model.answer = Aws.GenModel4mDS(Answers_ds: Aws.GetAnswersForQuestion(null, QuestionId));
                    }
                    else
                    {
                        model.answer = Answer.InitializeIfNone(model.answer);
                    }

                }

                if (TempData["StatusMsg"] == null)
                {
                    TempData["Title"] = model.question.Q_Title;
                }
                else
                {
                    TempData["Title"] = TempData["StatusMsg"];
                }
                if (Request.IsAuthenticated)
                {
                    TempData["LoggedInUser"] = User.Identity.GetCurUserNumber();
                }
                else
                {
                    TempData["LoggedInUser"] = 0;
                }

                return View(_View, model);

            //TempData["ErrorPrevention"] = "Maybe the data required to display the question was incomplete. Do you want to <a href='/Questions/Create'> ask a question </a>?";
            //ModelState.AddModelError("", "An error occured while displaying question");
            //return View("Error");

        }

        // GET: Questions/Create
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Create()
        {
            TempData["Title"] = "Ask question!";

            TempData["UrlReferrer"] = this.GetRequestReferrer();
            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";

                return View("~/Views/Account/Login.cshtml");
            }
            //TempData["Guideline"] = "Please be specific and clear";

            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //, ExportModelStateToTempData
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize, ValidateAntiForgeryToken]
        public ActionResult Create(QAModel model)
        {

            Question question = model.question;

            try
            {
                QuestionWS Qws = new QuestionWS();
                DBUtil objDBUtil = new DBUtil(3);

                DataSet AddQuestion = Qws.WriteQuestion(question, 1, this);
                if (AddQuestion.RowsExists())
                {

                    int resTbl = AddQuestion.Tables.Count - 1;
                    TempData["StatusMsg"] = AddQuestion.Tables[resTbl].Rows[0]["StatusMsg"].ToString();
                    if (int.Parse(AddQuestion.Tables[resTbl].Rows[0]["StatusCode"].ToString()) == 1)
                    {
                        question.QuestionId = int.Parse(AddQuestion.Tables[resTbl].Rows[0]["QuestionId"].ToString());
                    }
                    else if (int.Parse(AddQuestion.Tables[resTbl].Rows[0]["StatusCode"].ToString()) == 9)
                    {
                        question.QuestionId = int.Parse(AddQuestion.Tables[resTbl].Rows[0]["Duplicate QuestionId"].ToString());
                    }

                    return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });

                }


            }
            catch { }

            ModelState.AddModelError("", "An error occured while adding the question");
            return View(model);
        }


        // GET: Questions/Edit/5
        [AcceptVerbs(HttpVerbs.Get)]
        //[Authorize]
        public ActionResult Edit(int QuestionId)
        {
            TempData["UrlReferrer"] = this.GetRequestReferrer();
            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";

                return View("~/Views/Account/Login.cshtml");
            }

            QAModel model = new QAModel();

            TempData["Title"] = "Improve question!";

            QuestionWS QWS = new QuestionWS();
            model.question = QWS.GetQuestionById(QuestionId, this.GetVisitorIP(Session["VisitorIP"]));

            return View(model);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize, ValidateAntiForgeryToken]
        public ActionResult Update(QAModel model)
        {
            //try
            //{
                Question question = model.question;

                QuestionWS Qws = new QuestionWS();
                DBUtil objDBUtil = new DBUtil(3);

                DataSet AddQuestion = Qws.WriteQuestion(question, 2, this);

                if (AddQuestion.RowsExists())
                {
                    //taking from 0th table because it will accomodate updates from other users as well

                    if (int.Parse(AddQuestion.Tables[0].Rows[0]["StatusCode"].ToString()) == 1)
                    {
                        TempData["StatusMsg"] = AddQuestion.Tables[0].Rows[0]["StatusMsg"].ToString();
                    }

                    return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });

                }
            //}
            //catch { }

            ModelState.AddModelError("", "An error occured while updating the question");
            return RedirectPermanent(this.GetRequestReferrer());
        }


        // GET: Answers/Delete/5
        [Authorize]
        public ActionResult Delete(int QuestionId)
        {
            DBUtil dBUtil = new DBUtil(3);
            MySqlCommand cmd = new MySqlCommand("spRemoveQuestion");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQuestionId", QuestionId);

            DataSet dsRemoveQuestion = dBUtil.FillDataSet(cmd);

            if (dsRemoveQuestion.RowsExists())
            {
                DataTable dtRemoveQuestion = dsRemoveQuestion.Tables[0];
                TempData["StatusMsg"] = dtRemoveQuestion.Rows[0]["StatusMsg"].ToString();
            }

            return RedirectToActionPermanent("Index", "Questions");
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Question question = await db.Question.FindAsync(id);
            db.Question.Remove(question);
            await db.SaveChangesAsync();
            return RedirectToActionPermanent("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
