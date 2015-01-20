using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace MicroBlog.Models
{
	public class BlogContext : DbContext
	{
		public DbSet<UserModel> Users { get; set; }
		public DbSet<PostModel> Posts { get; set; }

		public BlogContext() : base()
		{
			Database.Log = text => System.Diagnostics.Debug.WriteLine(text);
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserModel>().HasMany<UserModel>(user => user.Follows)
				.WithMany(followed => followed.Followers)
				.Map(conf =>
				{
					conf.MapLeftKey("follower_user_id");
					conf.MapRightKey("followed_user_id");
					conf.ToTable("Followed");
				});
		}
	}
}