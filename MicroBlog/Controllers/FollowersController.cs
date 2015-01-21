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
	/// <summary>
	/// Controller to work with user's followers
	/// </summary>
	public class FollowersController : ApiController
    {

		private BlogContext db = new BlogContext();

		/// <summary>
		/// Add follower to user
		/// </summary>
		/// <param name="userId">Id of a <see cref="UserModel">User</see> to add follower to</param>
		/// <param name="follower"><see cref="UserViewModel">User</see> to add as follower (either id or name may be empty)</param>
		/// <returns><see cref="IHttpActionResult">ActionResult</see> of the operation containing <see cref="UserViewModel"/> of the follower</returns>
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

		/// <summary>
		/// Delete the follower with the id specified from the list of user's followers
		/// </summary>
		/// <param name="userId">Id of the user to remove follower from</param>
		/// <param name="id">Id of the user to remove from the list of followers</param>
		/// <returns><see cref="IHttpActionResult">ActionResult</see> of the operation containing <see cref="UserViewModel"/> of the follower</returns>
		[ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> DeleteFollowers(int userId, int id)
		{
			var user =await db.Users.FindAsync(userId);
			
			var follower = await db.Users.FindAsync(id);
			if (follower == null || user == null)
				return NotFound();

			user.Followers.Remove(follower);
			await db.SaveChangesAsync();
			return Ok(new UserViewModel
			{
				Id = follower.Id,
				Name = follower.Name
			});
		}

		/// <summary>
		/// Gets the list of the followers for the specified user
		/// </summary>
		/// <param name="userId">Id of the user to get followers for</param>
		/// <returns>List of the <see cref="UserViewModel">followers</see></returns>
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
