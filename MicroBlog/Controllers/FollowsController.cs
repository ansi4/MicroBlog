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
	/// Controller to work with followed users
	/// </summary>
    public class FollowsController : ApiController
    {
		private BlogContext db = new BlogContext();


		#region UserFollows

		/// <summary>
		/// Add user to the list of users followed
		/// </summary>
		/// <param name="userId">Id of the <see cref="UserModel">User</see>  to add followed user</param>
		/// <param name="followed"><see cref="UserViewModel">User</see> to follow</param>
		/// <returns><see cref="IHttpActionResult">ActionResult</see> of the operation containing <see cref="UserViewModel"/> of the follower</returns>
		[ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> PostFollows(int userId, UserViewModel followed)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			UserModel currentUser = await db.Users.FindAsync(userId);
			UserModel followedUser;
			if (followed.Id != 0)
				followedUser = await db.Users.FindAsync(followed.Id);
			else
				followedUser = await db.Users.Include(u => u.Follows).SingleAsync(u => u.Name == followed.Name);

			if (currentUser == null || followedUser == null)
			{
				return NotFound();
			}

			currentUser.Follows.Add(followedUser);
			
			try
			{
				await db.SaveChangesAsync();
			}
			catch (Exception)
			{
				throw;
			}

			return CreatedAtRoute("DefaultApi", new { id = userId, followed = followed }, UserViewModel.FromUser(followedUser));
		}

		[ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> DeleteFollows(int userId, int id)
		{
			var user =
				db.Users.Include(u => u.Follows).SingleOrDefault(u => u.Id == userId);
			if (user == null)
				return NotFound();
			var followed = user.Follows.SingleOrDefault(u => u.Id == id);
			if (followed == null)
				return NotFound();

			user.Follows.Remove(followed);
			await db.SaveChangesAsync();
			return Ok(UserViewModel.FromUser(followed));
		}

		public IQueryable<UserViewModel> GetFollows(int userId)
		{
			return db.Users.Include(u => u.Follows)
				.Where(u => u.Id == userId)
				.SelectMany(u => u.Follows, (user, followed) => new UserViewModel()
				{
					Id = followed.Id,
					Name = followed.Name
				});
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
