using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace UI.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        #region Custom helpers

        [Required]
        public override string UserName { get; set; }

        [Required]
        public override string Email { get; set; }

        public int UserNumber { get; set; }
        public string ProfileUrl { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public DateTime DOB { get; set; }
        public string InterestedFields { get; set; }
        public string Title { get; set; }
        public int MarksScore { get; set; }
        public int SchoolIds { get; set; }
        public int BadgeCount { get; set; }
        public int TrophyCount { get; set; }
        public int CertsCount { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public int ClassmateCount { get; set; }
        public string Bio { get; set; }
        public string BookIds { get; set; }
        public string InterestsIds { get; set; }
        public string TestimonialsIds { get; set; }
        public string CertIds { get; set; }
        public string AwardIds { get; set; }
        public string EventIds { get; set; }
        public string TeamIds { get; set; }
        public DateTime Joined { get; set; }
        public int UserAcStatus { get; set; }

        [DisplayName("Display status (eg: studying, exam time etc.)")]
        public string DisplayStatus { get; set; }
        public string Website { get; set; }
        public int SocialLinksId { get; set; }

        public List<ApplicationUser> AppUsers = new List<ApplicationUser>();

        public ApplicationUser GenModel4mDS(DataSet dsAppUser)
        {
            ApplicationUser AppUser = new ApplicationUser();
            AppUser.AppUsers = new List<ApplicationUser>();
            if (dsAppUser.RowsExists())
            {
                DataTable dtAppUser = new DataTable();
                for (int i = 0; i < dtAppUser.Rows.Count; i++)
                {
                    ApplicationUser item = new ApplicationUser();
                    item.AccessFailedCount = int.Parse(dtAppUser.Rows[i]["AccessFailedCount"].ToString());
                    item.Address = dtAppUser.Rows[i]["Address"].ToString();
                    item.AwardIds = dtAppUser.Rows[i]["AwardIds"].ToString();
                    item.BadgeCount = int.Parse(dtAppUser.Rows[i]["BadgeCount"].ToString());
                    item.Bio = dtAppUser.Rows[i]["Bio"].ToString();
                    item.BookIds = dtAppUser.Rows[i]["BookIds"].ToString();
                    item.CertIds = dtAppUser.Rows[i]["CertIds"].ToString();
                    item.CertsCount = int.Parse(dtAppUser.Rows[i]["CertsCount"].ToString());
                    item.ClassmateCount = int.Parse(dtAppUser.Rows[i]["ClassmateCount"].ToString());
                    item.DisplayStatus = dtAppUser.Rows[i]["DisplayStatus"].ToString();
                    item.DOB =DateTime.Parse(  dtAppUser.Rows[i]["DOB"].ToString());
                    item.Email = dtAppUser.Rows[i]["Email"].ToString();
                    item.EventIds = dtAppUser.Rows[i]["EventIds"].ToString();
                    item.FName = dtAppUser.Rows[i]["FName"].ToString();
                    item.Id = dtAppUser.Rows[i]["Id"].ToString();
                    item.InterestedFields = dtAppUser.Rows[i]["InterestedFields"].ToString();
                    item.InterestsIds = dtAppUser.Rows[i]["InterestsIds"].ToString();
                    item.Joined = DateTime.Parse( dtAppUser.Rows[i]["Joined"].ToString());
                    item.LName = dtAppUser.Rows[i]["LName"].ToString();
                    item.Location = dtAppUser.Rows[i]["Location"].ToString();
                    item.MarksScore = int.Parse( dtAppUser.Rows[i]["MarksScore"].ToString());
                    item.MName = dtAppUser.Rows[i]["MName"].ToString();
                    item.PhoneNumber = dtAppUser.Rows[i]["PhoneNumber"].ToString();
                    item.ProfileUrl = dtAppUser.Rows[i]["ProfileUrl"].ToString();
                    item.SchoolIds =  int.Parse( dtAppUser.Rows[i]["SchoolIds"].ToString());
                    item.SocialLinksId = int.Parse( dtAppUser.Rows[i]["SocialLinksId"].ToString());
                    item.TeamIds = dtAppUser.Rows[i]["TeamIds"].ToString();
                    item.TestimonialsIds = dtAppUser.Rows[i]["TestimonialsIds"].ToString();
                    item.Title = dtAppUser.Rows[i]["Title"].ToString();
                    item.TrophyCount = int.Parse(dtAppUser.Rows[i]["TrophyCount"].ToString());
                    item.UserAcStatus = int.Parse(dtAppUser.Rows[i]["UserAcStatus"].ToString());
                    item.UserName =  dtAppUser.Rows[i]["UserName"].ToString();
                    item.UserNumber = int.Parse(dtAppUser.Rows[i]["UserNumber"].ToString());
                    item.Website = dtAppUser.Rows[i]["Website"].ToString();

                    AppUser.AppUsers.Add(item);
                }
            }

            return AppUser;
        }

        #endregion

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}