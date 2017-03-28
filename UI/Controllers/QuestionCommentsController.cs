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
using MySql.Data.MySqlClient;

namespace UI.Controllers
{
    public class QuestionCommentsController : Controller
    {
        private QuestionDbContext db = new QuestionDbContext();

        MySqlCommand cmd = new MySqlCommand();
        DBUtil dbUtil = new DBUtil();
        UrlUtil url = new UrlUtil();

        // GET: QuestionComments
        public async Task<ActionResult> Index()
        {
            return View(await db.Comments.ToListAsync());
        }

        // GET: QuestionComments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionComment questionComment = (QuestionComment)await db.Comments.FindAsync(id);
            if (questionComment == null)
            {
                return HttpNotFound();
            }
            return View(questionComment);
        }

        /* GET: QuestionComments/Create
        [HttpGet]
        public ActionResult Create(QAModel model)
        {
            return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
        }
        */

        //TODO: prevent over posting across application
        // POST: QuestionComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QAModel model) // int pQuestionId, string pCommentTxt, int pCommentBy
        {

            if( model.question.questionComment.CommentTxt.Length>500)
            {
                TempData["StatusMsg"] = "Sorry! comments cannot be more than 500 charachters";
                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }

            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";
                TempData["UrlReferrer"] = this.GetRequestReferrer();

                return View("~/Views/Account/Login.cshtml");
            }

            dbUtil = new DBUtil(3);
            cmd = new MySqlCommand("spAddQuestionComment");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pQuestionId", model.question.QuestionId);
            cmd.Parameters.AddWithValue("pCommentTxt", model.question.questionComment.CommentTxt.SanitizeInput());
            cmd.Parameters.AddWithValue("pCommentTxtHtml", model.question.questionComment.CommentTxtHtml.ConvertMdToHtml());
            cmd.Parameters.AddWithValue("pCommentBy", User.Identity.GetCurUserNumber());
            DataSet AddQuestionComment = dbUtil.FillDataSet(cmd);
            if (AddQuestionComment.RowsExists())
            {

                int resTbl = AddQuestionComment.Tables.Count - 1;
                    TempData["StatusMsg"] = AddQuestionComment.Tables[resTbl].Rows[0]["StatusMsg"].ToString();

                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }

            return View("Error");
        }

        // GET: QuestionComments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionComment questionComment = (QuestionComment)await db.Comments.FindAsync(id);
            if (questionComment == null)
            {
                return HttpNotFound();
            }
            return View(questionComment);
        }

        // POST: QuestionComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(QAModel model)
        {
            if (model.question.questionComment.CommentTxt.Length > 500)
            {
                TempData["StatusMsg"] = "Sorry! comments cannot be more than 500 charachters";
                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }

            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";
                TempData["UrlReferrer"] = this.GetRequestReferrer();

                return View("~/Views/Account/Login.cshtml");
            }

            dbUtil = new DBUtil(3);
            cmd = new MySqlCommand("spUpdateQuestionComment");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pCommentId", model.question.questionComment.CommentId);
            cmd.Parameters.AddWithValue("pQuestionId", model.question.QuestionId);
            cmd.Parameters.AddWithValue("pCommentTxt", model.question.questionComment.CommentTxt.SanitizeInput());
            cmd.Parameters.AddWithValue("pCommentTxtHtml", model.question.questionComment.CommentTxtHtml.ConvertMdToHtml());
            cmd.Parameters.AddWithValue("pCommentBy", User.Identity.GetCurUserNumber());
            DataSet AddQuestionComment = dbUtil.FillDataSet(cmd);
            if (AddQuestionComment.RowsExists())
            {

                int resTbl = AddQuestionComment.Tables.Count - 1;
                TempData["StatusMsg"] = AddQuestionComment.Tables[resTbl].Rows[0]["StatusMsg"].ToString();

                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }

            return View("Error");
        }

        // GET: QuestionComments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionComment questionComment = (QuestionComment)await db.Comments.FindAsync(id);
            if (questionComment == null)
            {
                return HttpNotFound();
            }
            return View(questionComment);
        }

        // POST: QuestionComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            QuestionComment questionComment = (QuestionComment)await db.Comments.FindAsync(id);
            db.Comments.Remove(questionComment);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
