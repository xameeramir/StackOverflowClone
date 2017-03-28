using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UI.Models;
using System.Web.Routing;
using MySql.Data.MySqlClient;

namespace UI.Controllers
{
    public class AnswersController : Controller
    {
        private AnswersContext db = new AnswersContext();
        UrlUtil url = new UrlUtil();

        [AllowAnonymous]
        public ActionResult AnswersByUser(int A_By, string A_ByUName)
        {
            QAModel model = new QAModel();

            AnswerWS Aws = new AnswerWS();
            model.answer = Aws.AnswersByUser(A_By);

            TempData["Title"] = string.Format("Answers by {0}", A_ByUName);

            return View("Index", model);

        }

        // GET: Answers
        [AllowAnonymous]
        [ExportModelStateToTempData]
        public ActionResult Index(int QuestionId, string Sort = "")
        {
            Answer model = new Answer();
            try
            {
                if (model.answers == null && QuestionId > 0)
                {
                    string procName;

                    switch (Sort)
                    {
                        default:
                        case "Top":
                            procName = "spGetTopAnswersByQ_Id";
                            break;

                        case "Live":
                            procName = "spGetLiveAnswerByQ_Id";
                            break;

                        case "Oldest":
                            procName = "spGetOldestAnswerByQ_Id";
                            break;

                        case "Newest":
                            procName = "spGetAllAnswerByQ_Id";
                            break;
                    }

                    AnswerWS Aws = new AnswerWS();
                    model = Aws.GenModel4mDS(
                        Answers_ds: Aws.GetAnswersForQuestion(procName, QuestionId));

                }

                ViewBag.Filter = Sort;
                return View(model);

            }
            catch (Exception ex)
            {
                new ErrorUtil().LogException(ex, 10, "Some error occured while retrieving answers");
            }


            TempData["ErrorPrevention"] = "Do you want to <a href='/Answers/Create?QuestionId=" + QuestionId + "'> answer this question </a>?";
            ModelState.AddModelError("", "An error occured while retrieving answers");
            return View("Error");
        }

        // GET: Answers/Details/5
        /// <summary>
        /// Single reusable action for displaying answer
        /// </summary>
        /// <param name="AnswerCount"></param>
        /// <param name="model">The answer to display</param>
        /// <param name="_View">Eg: Details for display, Edit for editing, Delete for deleting </param>
        /// <returns>The view specified by _View</returns>
        public ActionResult Details(int AnswerId, QAModel model, string _View)
        {
            try
            {
                AnswerWS Aws = new AnswerWS();

                if (AnswerId > 0)
                {
                    model.answer = Aws.GetAnswerById(AnswerId, this.GetVisitorIP(Session["VisitorIP"]));
                }
                else if (model.question.QuestionId > 0)
                {
                    model.answer = Aws.GetAnswersForQuestion(model.question.QuestionId);
                }

                return View(_View, model);
            }
            catch (Exception)
            {
                //TODO: log this exception
            }

            TempData["ErrorPrevention"] = "Maybe the data required to display the answer was incomplete. Do you want to <a href='/Questions/Create'> ask a question </a>?";
            ModelState.AddModelError("", "An error occured while displaying answer");
            return View("Error");

        }

        // GET: Answers/Create
        [Authorize]
        public ActionResult Create()
        {
            TempData["Guideline"] = "Please be specific and clear";

            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";
                return View("~/Views/Account/Login.cshtml");
            }

            return View();
        }

        // POST: Answers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [AcceptVerbs(HttpVerbs.Post), ExportModelStateToTempData]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QAModel model)
        {
                AnswerWS Aws = new AnswerWS();

                if (model.answer.QuestionId == 0)
                {
                    model.answer.QuestionId = model.question.QuestionId;
                }

                TempData["StatusMsg"] = Aws.WriteAnswer(model.answer, 1, this);

                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.answer.QuestionId });

            //ModelState.AddModelError("", "An error occured while adding the answer");
            //return View(model);
        }

        // GET: Answers/Edit/5
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(int AnswerId)
        {

            TempData["UrlReferrer"] = this.GetRequestReferrer();

            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";

                return View("~/Views/Account/Login.cshtml");
            }

            Answer model = new Answer();

            TempData["Title"] = "Improve answer!";
            //TempData["Guideline"] = "Thanks for your contribution";

            AnswerWS AWS = new AnswerWS();
            model = AWS.GetAnswerById(AnswerId, this.GetVisitorIP(Session["VisitorIP"]));
            

            return View(model);
        }

        [Authorize, ValidateAntiForgeryToken, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(QAModel model)
        {
            try
            {
                Answer answer = model.answer;

                AnswerWS Aws = new AnswerWS();
                DBUtil objDBUtil = new DBUtil(3);

                TempData["StatusMsg"] = Aws.WriteAnswer(answer, 2, this);

                if (TempData["UrlReferrer"] != null)
                {
                    return RedirectPermanent(TempData["UrlReferrer"].ToString());
                }

                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }
            catch { }

            ModelState.AddModelError("", "An error occured while updating the answer");
            return RedirectPermanent(this.GetRequestReferrer());
        }

        // GET: Answers/Delete/5
        [Authorize]
        public ActionResult Delete(int AnswerId, int QuestionId)
        {
            DBUtil dBUtil = new DBUtil(3);
            MySqlCommand cmd = new MySqlCommand("spRemoveAnswer");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pAnswerId", AnswerId);
            cmd.Parameters.AddWithValue("pQuestionId", QuestionId);
            DataSet dsRemoveAnswer = dBUtil.FillDataSet(cmd);

            if (dsRemoveAnswer.RowsExists())
            {
                DataTable dtRemoveAnswer = dsRemoveAnswer.Tables[0];
                TempData["StatusMsg"] = dtRemoveAnswer.Rows[0]["StatusMsg"].ToString();
            }

            return RedirectToActionPermanent("Details", "Questions", new { QuestionId = QuestionId });
        }

        // POST: Answers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Answer answer = await db.Answers.FindAsync(id);
            db.Answers.Remove(answer);
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
