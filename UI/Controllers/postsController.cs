using System;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using UI.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using UI.WS;

namespace UI.Controllers
{
    public class postsController : Controller
    {
        private postDBContexts db = new postDBContexts();
        UrlUtil url = new UrlUtil();

        /// <summary>
        /// For providing autocomplete field suggestion
        /// </summary>
        /// <param name="term"> Field starting with term </param>
        /// <returns> Field name suggestions </returns>
        public ActionResult GetFieldStartingWith(string term)
        {
            // TODO: Get fields from database
            try
            {
                DBUtil objDBUtil = new DBUtil(dbID: 2);

                MySqlCommand cmd = new MySqlCommand("spGetFieldStartingWith");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_term", term);

                DataSet dsFieldStartingWith = objDBUtil.FillDataSet(cmd);
                if (dsFieldStartingWith.RowsExists())
                {

                    int resTbl = dsFieldStartingWith.Tables.Count - 1;
                    {
                        string json = JsonConvert.SerializeObject(dsFieldStartingWith, Formatting.Indented /*or new DataSetConverter()*/);
                        //dsAddField = JsonConvert.DeserializeObject<DataSet>(json);
                        return Json(json, JsonRequestBehavior.AllowGet);
                        //return Json(dsFieldStartingWith.Tables[resTbl].Rows, JsonRequestBehavior.AllowGet);

                    }
                }


            }
            catch { }

            return Json("An error occured while retrieving fields", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddField(string fieldName, string fieldWiki)
        {
            try
            {
                fieldName = "#" + fieldName; //making sure that it looks like a hashtag
                DBUtil objDBUtil = new DBUtil(dbID: 2);

                MySqlCommand cmd = new MySqlCommand("spAddField");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("pfieldName", fieldName);
                cmd.Parameters.AddWithValue("pfieldCreatedBy", User.Identity.GetCurUserNumber());
                cmd.Parameters.AddWithValue("pfieldCreatedByUName", User.Identity.Name);
                cmd.Parameters.AddWithValue("pfieldWiki", fieldWiki);

                DataSet dsAddField = objDBUtil.FillDataSet(cmd);
                if (dsAddField.RowsExists())
                {

                    int resTbl = dsAddField.Tables.Count - 1;
                    if (int.Parse(dsAddField.Tables[resTbl].Rows[0]["StatusCode"].ToString()) != 0)
                    {
                        return Json(dsAddField.Tables[resTbl].Rows[0]["StatusMsg"].ToString(), JsonRequestBehavior.AllowGet);
                    }
                }


            }
            catch { }

            return Json("An error occured while adding field", JsonRequestBehavior.AllowGet);
        }


        //
        //postMarks
        public ActionResult postMarks(string returnUrl, Marks model, int MarksType, int postId, int postType, int postBy)
        {

            if (!Request.IsAuthenticated)
            {
                TempData["RedirectMsg"] = "You must be logged in first";
                return View("~/Views/Account/Login.cshtml");
            }

            try
            {
                MarksWS marksWS = new MarksWS();

                model.MarksBy = User.Identity.GetCurUserNumber();

                if (postBy == model.MarksBy)
                {
                    TempData["StatusMsg"] = "Sorry, you cannot award marks to your own post";
                }

                else
                {
                    TempData["StatusMsg"] = marksWS.AwardMarksForPost(postType, MarksType, postId, model.MarksBy, postBy);
                }

                //TODO : return as status massage after using AJAX or like
                //return Json(marksWS.AwardMarksForPost(postType, MarksType, postId, model.MarksBy, postBy), JsonRequestBehavior.AllowGet);
                return url.RedirectToLocal(this, returnUrl);

            }

            catch (Exception ex)
            {
                new ErrorUtil().LogException(ex, 5, "An error occured while awarding marks");
            }

            ModelState.AddModelError("", "An error occured while awarding marks");
            return View("Error");

        }

        // GET: posts
        public async Task<ActionResult> Index()
        {
            return View(await db.post.ToListAsync());
        }

        // GET: posts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            post post = await db.post.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: posts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "postId,postType,postTitle,postWiki,postBy,postDate,postEditBy,postEditDate,postByUname,postEditByUname,postMarksEarned,postPremiums,postPremiumsBy,postPremiumsByUname,postQualityEarned,postShares,postStatus,postStatusBy,postStatusByUname,postF1,postF2,postF3,postF4,postF5,postFlags,postFlagsBy,postFlagsByUname")] post post)
        {
            if (ModelState.IsValid)
            {
                db.post.Add(post);
                await db.SaveChangesAsync();
                return RedirectToActionPermanent("Index");
            }

            return View(post);
        }

        // GET: posts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            post post = await db.post.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "postId,postType,postTitle,postWiki,postBy,postDate,postEditBy,postEditDate,postByUname,postEditByUname,postMarksEarned,postPremiums,postPremiumsBy,postPremiumsByUname,postQualityEarned,postShares,postStatus,postStatusBy,postStatusByUname,postF1,postF2,postF3,postF4,postF5,postFlags,postFlagsBy,postFlagsByUname")] post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToActionPermanent("Index");
            }
            return View(post);
        }

        // GET: posts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            post post = await db.post.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            post post = await db.post.FindAsync(id);
            db.post.Remove(post);
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
