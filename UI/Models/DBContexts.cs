using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace UI.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //public System.Data.Entity.DbSet<UI.Models.Question> Questions { get; set; }

        //public System.Data.Entity.DbSet<UI.Models.ApplicationUser> ApplicationUsers { get; set; }
    }

    public class QuestionDbContext : DbContext
    {
        public QuestionDbContext()
            : base("dbQuestions")
        {
        }

        public System.Data.Entity.DbSet<UI.Models.Question> Question { get; set; }

        public System.Data.Entity.DbSet<UI.Models.Comment> Comments { get; set; }
    }

    public class postDBContexts : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public postDBContexts(): base("dbPosts")
        {
        }

        public System.Data.Entity.DbSet<UI.Models.post> post { get; set; }

        public System.Data.Entity.DbSet<UI.Models.Marks> Marks { get; set; }
    
    }

    public class AnswersContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public AnswersContext()
            : base("dbQuestions")
        {
        }

        public System.Data.Entity.DbSet<UI.Models.Answer> Answers { get; set; }

        public System.Data.Entity.DbSet<UI.Models.Question> Questions { get; set; }

    }

}
