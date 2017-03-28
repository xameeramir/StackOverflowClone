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
    public class AnswerCommentsController : Controller
    {
        private QuestionDbContext db = new QuestionDbContext();
        DBUtil dbUtil = null;
        MySqlCommand cmd = null;

        // GET: AnswerComments
        public async Task<ActionResult> Index()
        {
            return View(await db.Comments.ToListAsync());
        }

        // GET: AnswerComments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerComment answerComment = (AnswerComment)await db.Comments.FindAsync(id);
            if (answerComment == null)
            {
                return HttpNotFound();
            }
            return View(answerComment);
        }

        /* GET: AnswerComments/Create
        public ActionResult Create()
        {
            return View();
        }*/

        // POST: AnswerComments/Create
        // POST: QuestionComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QAModel model) // int pQuestionId, string pCommentTxt, int pCommentBy
        {

            if (model.answer.answerComment.CommentTxt.Length > 500)
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
            cmd = new MySqlCommand("spAddAnswerComment");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pAnswerId", model.answer.AnswerId);
            cmd.Parameters.AddWithValue("pCommentTxt", model.answer.answerComment.CommentTxt.SanitizeInput());
            cmd.Parameters.AddWithValue("pCommentTxtHtml", model.answer.answerComment.CommentTxtHtml.ConvertMdToHtml());
            cmd.Parameters.AddWithValue("pCommentBy", User.Identity.GetCurUserNumber());
            DataSet AddAnswerComment = dbUtil.FillDataSet(cmd);
            if (AddAnswerComment.RowsExists())
            {
                int resTbl = AddAnswerComment.Tables.Count - 1;
                if (int.Parse(AddAnswerComment.Tables[resTbl].Rows[0]["StatusCode"].ToString()) == 1)
                {
                    TempData["StatusMsg"] = AddAnswerComment.Tables[resTbl].Rows[0]["StatusMsg"].ToString();
                }
                else if (int.Parse(AddAnswerComment.Tables[resTbl].Rows[0]["StatusCode"].ToString()) == 9)
                {
                    TempData["StatusMsg"] = AddAnswerComment.Tables[resTbl].Rows[0]["StatusMsg"].ToString();
                }

                return RedirectToActionPermanent("Details", "Questions", new { QuestionId = model.question.QuestionId });
            }

            return View("Error");
        }


        // GET: AnswerComments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerComment answerComment = (AnswerComment)await db.Comments.FindAsync(id);
            if (answerComment == null)
            {
                return HttpNotFound();
            }
            return View(answerComment);
        }

        // POST: AnswerComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CommentId,CommentTxt,CommentBy,CommentDt,CommentStatus,CommentStatusBy,CommentMarksEarned,CommentEdits,CommentType,postType,AnswerId")] AnswerComment answerComment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(answerComment).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(answerComment);
        }

        // GET: AnswerComments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerComment answerComment = (AnswerComment)await db.Comments.FindAsync(id);
            if (answerComment == null)
            {
                return HttpNotFound();
            }
            return View(answerComment);
        }

        // POST: AnswerComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AnswerComment answerComment = (AnswerComment)await db.Comments.FindAsync(id);
            db.Comments.Remove(answerComment);
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
