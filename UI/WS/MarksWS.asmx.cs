using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Services;
using MySql.Data.MySqlClient;
using UI.Models;

namespace UI.WS
{
    /// <summary>
    /// Summary description for MarksWS
    /// </summary>
    [WebService(Namespace = "http://knowrdible.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class MarksWS : System.Web.Services.WebService
    {

        public Marks GenModel4mDS(DataSet dsMarks)
        {
            DBUtil objDBUtil = new DBUtil(dbID: 2);

            Marks model = new Marks();
            if (dsMarks.RowsExists())
            {

                for (int i = 0; i < dsMarks.Tables[0].Rows.Count; )
                {
                    if (dsMarks.Tables[0].Rows[0]["MarksId"].ToString() != "")
                    {
                        model.MarksAwarded = int.Parse(dsMarks.Tables[0].Rows[i]["MarksAwarded"].ToString());
                        model.MarksType = int.Parse(dsMarks.Tables[0].Rows[i]["MarksType"].ToString());
                        model.MarksBy = int.Parse(dsMarks.Tables[0].Rows[i]["MarksBy"].ToString());
                        model.AwardedDt = Convert.ToDateTime(dsMarks.Tables[0].Rows[i]["AwardedDt"].ToString());
                        model.postType = int.Parse(dsMarks.Tables[0].Rows[i]["postType"].ToString());
                        return model;
                    }
                }
            }

            return null;
        }

        [WebMethod]
        public Marks GetMarksForPost(int postId, int postType)
        {
            DBUtil objDBUtil = new DBUtil(dbID: 2);
            MySqlCommand cmd = new MySqlCommand("spGetMarksForPost");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("ppostId", postId);
            cmd.Parameters.AddWithValue("postType", postType);
            DataSet dsMarks = objDBUtil.FillDataSet(cmd);
            return GenModel4mDS(dsMarks: dsMarks);
        }

        [WebMethod]
        public string AwardMarksForPost(int postType, int MarksType, int postId, int MarksBy, int postBy)
        {
                DBUtil objDBUtil = new DBUtil(dbID: 2);
                MySqlCommand cmd = new MySqlCommand("spAwardMarksToPost");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("ppostType", postType);
                cmd.Parameters.AddWithValue("pMarksType", MarksType);
                cmd.Parameters.AddWithValue("ppostId", postId);
                cmd.Parameters.AddWithValue("pMarksBy", MarksBy);
                cmd.Parameters.AddWithValue("ppostBy", postBy);
                
                DataSet postMarksds = objDBUtil.FillDataSet(cmd);
                if (postMarksds.RowsExists())
                {
                    int resTbl = postMarksds.Tables.Count - 1;
                    if (int.Parse(postMarksds.Tables[resTbl].Rows[0]["StatusCode"].ToString()) != 0)
                    {
                        return postMarksds.Tables[resTbl].Rows[0]["StatusMsg"].ToString();
                    }
                }

            return null;

        }

    }
}
