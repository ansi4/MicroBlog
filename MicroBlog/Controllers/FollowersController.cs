using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MicroBlog.Models;

namespace MicroBlog.Controllers
{
	[RoutePrefix("api/User/{userId:int}/Followers")]
	public class FollowersController : ApiController
    {

		private BlogContext db = new BlogContext();

		#region UserFollowers
		[ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> PostFollowers(int userId, UserViewModel follower)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var currUser = await db.Users.FindAsync(userId);
			UserModel followerUser;
			List<UserModel> users;
			if (follower.Id != 0)
				followerUser = await db.Users.FindAsync(follower.Id);
			else
				followerUser = await db.Users.Where(u => u.Name == follower.Name).SingleOrDefaultAsync();

			if (currUser == null || followerUser == null)
			{
				return BadRequest("{0} with the specified Id was not found");
			}

			currUser.Followers.Add(followerUser);
			try
			{
				await db.SaveChangesAsync();
			}
			catch (Exception)
			{
				throw;
			}

			return CreatedAtRoute("DefaultApi", new { id = userId, follower }, new UserViewModel { Id = followerUser.Id, Name = followerUser.Name });
		}

		[ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> DeleteFollowers(int userId, int id)
		{
			var user =
				db.Users.Include(u => u.Followers).SingleOrDefault(u => u.Id == userId);
			if (user == null)
				return NotFound();
			var follower = user.Followers.SingleOrDefault(u => u.Id == id);
			if (follower == null)
				return NotFound();

			user.Followers.Remove(follower);
			await db.SaveChangesAsync();
			return Ok(new UserViewModel
			{
				Id = follower.Id,
				Name = follower.Name
			});
		}

		public IQueryable<UserViewModel> GetFollowers(int userId)
		{
			return db.Users.Include(u => u.Followers).
				Where(u => u.Id == userId)
				.SelectMany(u => u.Followers, (user, follower) => new UserViewModel()
				{
					Id = follower.Id,
					Name = follower.Name
				}).OrderBy(u => u.Name);
		}



		#endregion
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
