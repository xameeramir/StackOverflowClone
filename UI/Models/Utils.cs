using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Net;
using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;
using System.IO;
using System.Drawing.Imaging;
using UI.Models;

namespace System.Data
{
    public class DBUtil
    {
        MySqlConnection con = new MySqlConnection();
        MySqlCommand cmd = new MySqlCommand();
        MySqlTransaction sqltrans;
        private static string
            //DB_Name = 0            
            dbErrorRecs = ConfigurationManager.ConnectionStrings["dbErrorRecs"].ToString(),
            //DB_Name = 1
            dbNpUsers = ConfigurationManager.ConnectionStrings["dbNpUsers"].ToString(),
            //DB_Name = 2            
            dbPosts = ConfigurationManager.ConnectionStrings["dbPosts"].ToString(),
            //DB_Name = 3            
            dbQuestions = ConfigurationManager.ConnectionStrings["dbQuestions"].ToString();

        public DBUtil()
        {
        }

        /// <summary>
        /// Initializes database utility
        /// </summary>
        /// <param name="dbID"> 0 : dbErrorRecs, 1: dbNpUsers, 2: dbPosts, 3: dbQuestions </param>
        public DBUtil(int dbID)
        {
            switch (dbID)
            {
                case 0:
                    con.ConnectionString = dbErrorRecs;
                    break;

                case 1:
                    con.ConnectionString = dbNpUsers;
                    break;

                case 2:
                    con.ConnectionString = dbPosts;
                    break;

                case 3:
                    con.ConnectionString = dbQuestions;
                    break;

                default:
                    throw new Exception("invalid database number");
            }

            cmd = con.CreateCommand();
        }

        private void OpenConnection()
        {
            try
            {
                if (con.State == ConnectionState.Closed || con.State == ConnectionState.Broken)
                {
                    con.Open();
                }
            }
            catch (Exception ex)
            {
                con.Dispose();
                throw new Exception(ex.ToString());
            }
        }

        public MySqlConnection GetConnection()
        {
            con = new MySqlConnection();
            OpenConnection();
            return con;
        }

        public void CloseConnection(MySqlConnection con)
        {
            try
            {
                con.Close();
            }
            catch (Exception)
            {
                con.Dispose();
            }
        }

        public int ExecuteNonQuery(string sql)
        {
            int rowAffected = -1;
            OpenConnection();
            cmd.Connection = con;
            sqltrans = con.BeginTransaction();
            cmd.Transaction = sqltrans;
            cmd.CommandText = sql;
            try
            {
                rowAffected = cmd.ExecuteNonQuery();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }
            return rowAffected;
        }

        private void CommitTransaction()
        {
            sqltrans.Commit();
            sqltrans = null;
        }

        private void RollbackTransaction()
        {
            sqltrans.Rollback();
            sqltrans = null;
        }

        public void VerifySpParams(MySqlCommand pcmd)
        {
            Console.WriteLine("There are " + pcmd.Parameters.Count + " parameters passed to " + pcmd.CommandText);

            for (int i = 0; i < pcmd.Parameters.Count; i++)
            {
                Console.WriteLine("Value: " + pcmd.Parameters[i].ParameterName);

                Console.WriteLine("Value: " + pcmd.Parameters[i].Value);
            }
        }

        public DataSet FillDataSet(MySqlCommand pcmd)
        {
            MySqlDataAdapter da = new MySqlDataAdapter();
            pcmd.Connection = con;
            DataSet ds = new DataSet();
            try
            {
                da.SelectCommand = pcmd;
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                throw new Exception("DBUtils.ExecuteReader():" + ex.Message);
            }
            finally
            {
                cmd.Dispose();
                da.Dispose();
            }

            return ds;
        }

        public void FillDataSet(string sql, ref DataSet ds)
        {
            MySqlDataAdapter da = new MySqlDataAdapter();
            OpenConnection();
            cmd.Connection = con;
            cmd.CommandText = sql;
            da.SelectCommand = cmd;
            try
            {
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                throw new Exception("DBUtils.FillDataSet():" + ex.Message);
            }
            finally
            {
                cmd.Dispose();
                da.Dispose();
            }
        }

        public DataTable GetDataTable(string table, string sql)
        {
            MySqlDataAdapter da = new MySqlDataAdapter();
            OpenConnection();
            cmd.Connection = con;
            cmd.CommandText = sql;
            da.SelectCommand = cmd;
            DataTable dt = new DataTable(table);
            try
            {
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("DBUtils.GetDataTable():" + ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                cmd.Dispose();
                da.Dispose();
            }
            return dt;
        }

        public MySqlDataReader ExecuteReader(string sql)
        {
            MySqlDataReader reader = null;
            OpenConnection();
            cmd.Connection = con;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception("DBUtils.ExecuteReader():" + ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }
            return reader;
        }

        public object ExecuteScalar(MySqlCommand cmd1)
        {
            object obj = null;
            OpenConnection();
            cmd1.Connection = con;
            //cmd.CommandText = sql; 
            try
            {
                obj = cmd1.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("DBUtils.ExecuteScalar():" + ex.Message);
            }
            finally
            {
                cmd1.Dispose();
            }
            return obj;
        }

        public int ExecuteCommand(MySqlCommand pcmd)
        {
            int rowsAffected = -1;
            OpenConnection();
            pcmd.Connection = con;
            sqltrans = con.BeginTransaction();
            pcmd.Transaction = sqltrans;
            try
            {
                rowsAffected = pcmd.ExecuteNonQuery();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                pcmd.Dispose();
            }
            return rowsAffected;
        }

        public Boolean TryDropTable(string tblName)
        {
            try
            {
                cmd = new MySqlCommand("sp_drop_tbl_IfExists");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tableName", tblName);
                ExecuteCommand(cmd);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public Boolean TryBulkCopy(string stringSource, string stringTarget, DataTable _DataTable, string _DestinationTableName, SqlBulkCopy paramBulkCopy)
        //{
        //    OpenConnection();
        //    try
        //    {
        //        if (paramBulkCopy == null)
        //        {
        //            DataAccess.GetMapping(stringSource, stringTarget, _DestinationTableName).ToList().ForEach(c => paramBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c, c)));
        //        }

        //        paramBulkCopy = new MySqlBulkCopy(con, SqlBulkCopyOptions.KeepIdentity, null) { DestinationTableName = _DestinationTableName };

        //        using (paramBulkCopy)
        //        {
        //            paramBulkCopy.WriteToServer(_DataTable);
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public int OutExecuteCommand(MySqlCommand pcmd)
        {
            object obj;
            int identity = -1;
            OpenConnection();
            pcmd.Connection = con;
            sqltrans = con.BeginTransaction();
            pcmd.Transaction = sqltrans;
            try
            {
                obj = pcmd.ExecuteScalar();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                pcmd.Dispose();
            }
            identity = Convert.ToInt32(obj);
            return identity;
        }

        public string StrOutExecuteCommand(MySqlCommand pcmd)
        {
            object obj;
            string name = "";
            OpenConnection();
            pcmd.Connection = con;
            sqltrans = con.BeginTransaction();
            pcmd.Transaction = sqltrans;
            try
            {
                obj = pcmd.ExecuteScalar();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                pcmd.Dispose();
            }
            name = Convert.ToString(obj);
            return name;
        }

        public int ExecuteCommandReturningID(MySqlCommand pcmd)
        {
            //int rowsAffected = -1; 
            Int32 ID = 0;
            OpenConnection();
            pcmd.Connection = con;
            sqltrans = con.BeginTransaction();
            pcmd.Transaction = sqltrans;
            try
            {
                ID = Convert.ToInt32(pcmd.ExecuteScalar());
                CommitTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                pcmd.Dispose();
            }
            return ID;
        }

    }

    public static class DataSetExtensions
    {
        /// <summary>
        /// Verifies wether the dataset not empty by counting rows and tables (at least 1 table with 1 row is required)
        /// </summary>
        /// <param name="_DataSet"> The dataset to be verified </param>
        /// <returns> Boolean </returns>
        public static Boolean RowsExists(this DataSet _DataSet)
        {
            #region Checks if rows exists in the table of the dataset

            if (_DataSet.Tables.Count > 0)
            {
                if (_DataSet.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
            }

            #endregion Checks if rows exists in the table of the dataset

            return false;
        }

    }


}

namespace System
{
    public static class StringExtensions
    {
        public static string SanitizeInput(this string inp)
        {
            return inp.EscapeApostrophe();
        }

        public static string SanitizeOutput(this string inp)
        {
            return inp.ReversingEscapeApostrophe();
        }
    }
}

namespace System.Web.Mvc
{
    public static class MarkdownDeepExtensions
    {
        public static void RenderMarkdown(this HtmlHelper helper, string text)
        {

            // Setup processor
            var md = new MarkdownDeep.Markdown
            {
                SafeMode = false,
                ExtraMode = true,
                AutoHeadingIDs = true,
                MarkdownInHtml = true,
                NewWindowForExternalLinks = true
            };

            // Write it
            helper.ViewContext.HttpContext.Response.Write(md.Transform(text));
        }

        public static string ConvertMdToHtml(this string text)
        {
            // Create and setup Markdown translator
            var md = new MarkdownDeep.Markdown();
            md.SafeMode = true;
            md.ExtraMode = true;

            // Transform the content and pass to the view
            return md.Transform(text);
        }

    }

    public static class RequestExtensions
    {
        public static string GetRequestReferrer(this Controller _Controller)
        {
            if (_Controller.Request.UrlReferrer != null)
            {
                return _Controller.Request.UrlReferrer.AbsoluteUri;
            }
            else
            {
                return _Controller.Request.Url.AbsoluteUri;
            }
        }
    }

    public static class HtmlHelperExtensions
    {
        public static IHtmlString GenerateIdenticon(this HtmlHelper html, string value, int dimension, bool useStaticBrush = false)
        {
            var i = new IdenticonGenerator()
                .WithBlockGenerators(IdenticonGenerator.ExtendedBlockGeneratorsConfig)
                .WithBrushGenerator(useStaticBrush ?
                    (IBrushGenerator)new StaticColorBrushGenerator(StaticColorBrushGenerator.ColorFromText(value)) : new RandomColorBrushGenerator()
                )
                .WithSize(dimension, dimension);
            using (var bitmap = i.Create(value))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                var img = new TagBuilder("img");
                img.Attributes.Add("width", bitmap.Width.ToString());
                img.Attributes.Add("height", bitmap.Height.ToString());
                img.Attributes.Add("src", String.Format("data:image/png;base64,{0}",
                    Convert.ToBase64String(stream.ToArray())));

                return MvcHtmlString.Create(img.ToString(TagRenderMode.SelfClosing));
            }
        }
    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the client IP address
        /// </summary>
        /// <param name="GetLan"> set to true if want to get local(LAN) connected IP address</param>
        /// <returns></returns>
        public static string GetVisitorIP(this Controller _Controller, object IPAddressInSession, bool GetLan = false)
        {
            IPAddress ip;
            string visitorIPAddress = "";
            if (IPAddressInSession != null)
            {
                if (IPAddress.TryParse(IPAddressInSession.ToString(), out ip))
                {
                    visitorIPAddress = IPAddressInSession.ToString();
                    return visitorIPAddress;
                }
            }

            visitorIPAddress = _Controller.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = _Controller.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = _Controller.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }

            if (GetLan && string.IsNullOrEmpty(visitorIPAddress))
            {
                //This is for Local(LAN) Connected ID Address
                string stringHostName = Dns.GetHostName();
                //Get Ip Host Entry
                IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                //Get Ip Address From The Ip Host Entry Address List
                IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                try
                {
                    visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                }
                catch
                {
                    try
                    {
                        visitorIPAddress = arrIpAddress[0].ToString();
                    }
                    catch
                    {
                        try
                        {
                            arrIpAddress = Dns.GetHostAddresses(stringHostName);
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            visitorIPAddress = "127.0.0.1";
                        }
                    }
                }

            }

            return visitorIPAddress;
        }
    }

    public class UrlUtil : Controller
    {
        public ActionResult RedirectToLocal(Controller _Controller, string returnUrl)
        {
            //returnUrl needs to be decoded
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = _Controller.Server.UrlDecode(returnUrl);

                if (!returnUrl.Contains("/Account/Login"))
                {

                    if (_Controller.Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                }
            }

            return RedirectToActionPermanent("Index", "Home");
        }


    }

    public static class SqlIOextensions
    {
        public static string EscapeApostrophe(this string inp)
        {
            if (inp != null)
            {
                return inp.Replace("'", "''");
            }
            else
            {
                return "";
            }
        }

        public static string ReversingEscapeApostrophe(this string inp)
        {
            if (inp != null)
            {
                return inp.Replace("''", "'");
            }
            else
            {
                return "";
            }
        }

    }

    //source: http://weblogs.asp.net/rashid/asp-net-mvc-best-practices-part-1#prg 13th item

    public abstract class ModelStateTempDataTransfer : ActionFilterAttribute
    {
        protected static readonly string Key = typeof(ModelStateTempDataTransfer).FullName;
    }

    public class ExportModelStateToTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Only export when ModelState is not valid
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                //Export if we are redirecting
                if ((filterContext.Result is RedirectResult) || (filterContext.Result is RedirectToRouteResult))
                {
                    filterContext.Controller.TempData[Key] = filterContext.Controller.ViewData.ModelState;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class ImportModelStateFromTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ModelStateDictionary modelState = filterContext.Controller.TempData[Key] as ModelStateDictionary;

            if (modelState != null)
            {
                //Only Import if we are viewing
                if (filterContext.Result is ViewResult)
                {
                    filterContext.Controller.ViewData.ModelState.Merge(modelState);
                }
                else
                {
                    //Otherwise remove it.
                    filterContext.Controller.TempData.Remove(Key);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class ErrorUtil
    {
        MySqlCommand cmd;
        DBUtil objDBUtil;

        public bool LogException(Exception ex, int eSeverity, string eComments)
        {
            objDBUtil = new DBUtil(0);
            cmd = new MySqlCommand("spLogError");
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("peMessage", ex.Message);
            cmd.Parameters.AddWithValue("peStackTrace", ex.StackTrace);
            cmd.Parameters.AddWithValue("peMethod", "Method"); //TODO: get last executed method name
            cmd.Parameters.AddWithValue("peFile", "peFile"); //TODO: get file name of exception error
            cmd.Parameters.AddWithValue("peFilePath", "peFilePath"); //TODO: get file path of exception error
            cmd.Parameters.AddWithValue("peLine", 0); //TODO: get file line of exception error
            cmd.Parameters.AddWithValue("peSeverity", eSeverity);
            cmd.Parameters.AddWithValue("peComments", eComments);
            objDBUtil.ExecuteCommand(cmd);

            return false;
        }
    }

    public class PageViewAttribute : ActionFilterAttribute
    {
        private static readonly TimeSpan pageViewDumpToDatabaseTimeSpan = new TimeSpan(0, 30, 0);

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var calledMethod = string.Format("{0} -> {1}",
                                             filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                                             filterContext.ActionDescriptor.ActionName);

            var cacheKey = string.Format("PV-{0}", calledMethod);

            var cachedResult = HttpRuntime.Cache[cacheKey];

            if (cachedResult == null)
            {
                HttpRuntime.Cache.Insert(cacheKey,
                    new PageViewValue(),
                    null,
                    DateTime.Now.Add(pageViewDumpToDatabaseTimeSpan),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Default,
                                      onRemove);
            }
            else
            {
                var currentValue = (PageViewValue)cachedResult;

                currentValue.Value++;
            }
        }

        private static void onRemove(string key, object value, CacheItemRemovedReason reason)
        {
            if (!key.StartsWith("PV-"))
            {
                return;
            }

            // write out the value to the database
        }

        // Used to get around weird cache behavior with value types
        public class PageViewValue
        {
            public PageViewValue()
            {
                Value = 1;
            }

            public int Value { get; set; }
        }
    }

    #region Extension methods

    public static class IdentityExtensions
    {
        /// <summary>
        /// Gives the user in the view usin System.Security.Principal.IIdentity instance
        /// </summary>
        /// <param name="_Identity"> IIdentity instance </param>
        /// <returns> The IIdentity user </returns>
        public static ApplicationUser GetCurUser(this IIdentity _Identity)
        {
            if (_Identity.IsAuthenticated == true)
            {
                // Create manager
                var manager = new UserManager<ApplicationUser>(
                   new UserStore<ApplicationUser>(
                       new ApplicationDbContext()));

                // Find user
                var user = manager.FindById(_Identity.GetUserId());

                return user;
            }

            return null;
        }

        public static ApplicationUser GetUserByUserName(this IIdentity _Identity, string UserName)
        {

            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var AppUser = manager.FindByName(UserName);

            return AppUser;
        }

        public static int GetCurUserNumber(this IIdentity _Identity)
        {
            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var user = manager.FindById(_Identity.GetUserId());

            //TODO: keep this in session, it's very expensive to query database everytime
            return user.UserNumber;

        }

        public static string AddHandleToUserName(this string UserName)
        {
            if ((char)UserName.ToCharArray().GetValue(0) != '@')
            {
                UserName = string.Format("@{0}", UserName);
            }

            return UserName;
        }

        public static string RemoveHandleFromUserName(this string UserName)
        {
            if ((char)UserName.ToCharArray().GetValue(0) == '@')
            {
                UserName = UserName.Substring(1);
            }

            return UserName;
        }

    }

    #endregion
}