using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Internal;
using MicroBlog.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MicroBlog.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MicroBlog.Models.BlogContext>
    {

		public Configuration()
        {
            AutomaticMigrationsEnabled = true;
	        AutomaticMigrationDataLossAllowed = true;
            ContextKey = "MicroBlog.Models.BlogModel";
        }

        protected override void Seed(MicroBlog.Models.BlogContext context)
        {
			var newUsers =	new string[50].Select((s, i) => new UserModel(){Name = "User" + i}).ToArray();
			context.Users.AddOrUpdate(model => model.Name, newUsers);
	        context.SaveChanges();
	        var posts = new List<PostModel>();
	        var rng = new Random();
	        foreach (var user in newUsers)
	        {
		        var max = rng.Next(5);
		        for (int i = 0; i < max; i++)
		        {
			        var toFollow = newUsers[rng.Next(newUsers.Length)];
					user.Follows.Add(toFollow);
		        }

		        var postCnt = rng.Next(10);
		        for (int i = 0; i < postCnt; i++)
		        {
			        posts.Add(new PostModel(){Author = user, Text=string.Format("User's {0} Post number {1}", user.Name, i), CreationTime = DateTime.Now});
		        }
	        }

			newUsers.ForEach(user => context.Users.AddOrUpdate(u => u.Id,user));
			posts.ForEach(post => context.Posts.AddOrUpdate(p => p.Text,  post));
        }
    }
}
