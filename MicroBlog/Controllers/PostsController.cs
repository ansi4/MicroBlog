using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using MicroBlog.Models;

namespace MicroBlog.Controllers
{
	/// <summary>
	/// Controller to operate on posts
	/// </summary>
	[RoutePrefix("api/User/{userId:int}")]
	public class PostsController : ApiController
    {
		private BlogContext db = new BlogContext();

		#region User Posts

		/// <summary>
		/// Add post
		/// </summary>
		/// <param name="userId">Id of the user  who created post</param>
		/// <param name="post"><see cref="PostViewModel">Post</see> details. Every property except Text should be ommited</param>
		/// <returns>New post details</returns>
		[ResponseType(typeof(PostViewModel))]
		[Route("Posts")]
		public async Task<IHttpActionResult> PostUserPost(int userId, PostViewModel post)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var author = await db.Users.FindAsync(userId);
			if (author == null)
				return NotFound();
			var postModel = new PostModel
			{
				Text = post.Text,
				CreationTime = DateTime.Now
			};
			author.Posts.Add(postModel);
			await db.SaveChangesAsync();

			post.CreationTime = postModel.CreationTime;
			post.Id = postModel.Id;
			post.Author = UserViewModel.FromUser(author);
			
			return CreatedAtRoute("DefaultApi", new { controller = "Posts", userId, id = post.Id }, post);
		}

		/// <summary>
		/// Fetches latest posts created by specified user
		/// </summary>
		/// <param name="userId">Id of the posts author</param>
		/// <param name="maxId">Last fetched post id. 0 to get most recent</param>
		/// <param name="limit">Post count to fetch</param>
		/// <returns>The list of user's <see cref="PostViewModel">Posts</see></returns>
		[Route("Posts")]
		public IQueryable<PostViewModel> GetUserPosts(int userId, long maxId = 0, int limit = 20)
		{
			var res = db.Posts.Include(p => p.Author)
				.Select(p => new PostViewModel()
				{
					Id = p.Id,
					Text = p.Text,
					CreationTime = p.CreationTime,
					Author = new UserViewModel()
					{
						Id = p.Author.Id,
						Name = p.Author.Name
					}
				}).Where(p => p.Author.Id == userId);
			if (maxId > 0)
			{
				res = res.Where(p => p.Id < maxId);
			}
			return res.OrderByDescending(p => p.Id).Take(limit);
		}

		/// <summary>
		/// Gets most recent posts of user's followers
		/// </summary>
		/// <param name="userId">User if to get followed users' posts for</param>
		/// <param name="maxId">Last post id retrieved. 0 to get most recent</param>
		/// <param name="limit">Post count to fetch</param>
		/// <returns>The list of followed users' <see cref="PostViewModel">Posts</see></returns>
		[Route("Follows/Posts")]
		public IQueryable<PostViewModel> GetPosts(int userId, long maxId = 0, int limit = 20)
		{
			var result =
				db.Users.Include(u => u.Follows)
					.Include(u => u.Posts)
					.Where(u => u.Id == userId)
					.SelectMany(u => u.Follows)
					.SelectMany(followed => followed.Posts).Select(p => new PostViewModel()
					{
						Id = p.Id,
						Text = p.Text,
						CreationTime = p.CreationTime,
						Author = new UserViewModel()
						{
							Id = p.Author.Id,
							Name = p.Author.Name
						}
					});
			if (maxId > 0)
				result = result.Where(post => post.Id < maxId);
			result = result.OrderByDescending(p => p.Id).Take(limit);

			return result;
		}

		#endregion

		// POST: api/Posts
		/// <summary>
		/// Publish new post
		/// </summary>
		/// <param name="postModel">Post data. CreationTime and Id could be omitted</param>
		/// <returns>New post details</returns>
		[ResponseType(typeof(PostViewModel))]
		[Route("~/api/Posts")]
		public async Task<IHttpActionResult> PostPosts(PostViewModel postModel)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			return await PostUserPost(postModel.Author.Id, postModel);
		}

		/// <summary>
		/// Gets most recent posts of all users
		/// </summary>
		/// <param name="maxId">Last post id retrieved. 0 to get most recent</param>
		/// <param name="limit">Post count to fetch</param>
		/// <returns>The list of most recent posts <see cref="PostViewModel">Posts</see></returns>
		[Route("~/api/Posts")]
		public IQueryable<PostViewModel> GetPosts(long maxId = 0, int limit = 20)
		{
			var resM = db.Posts.Include(post => post.Author);
			if (maxId > 0)
				resM = resM.Where(post => post.Id < maxId);
			var res =
				resM.Select(
					post =>
						new PostViewModel
						{
							Author = new UserViewModel { Id = post.Author.Id, Name = post.Author.Name },
							Id = post.Id,
							Text = post.Text,
							CreationTime = post.CreationTime
						}).OrderByDescending(post => post.Id).Take(limit);
			return res;
		}

		// DELETE: api/Posts/5
		/// <summary>
		/// Remove post with the specified id
		/// </summary>
		/// <param name="id">Id of the post to be removed</param>
		/// <returns>Removed post details</returns>
		[Route("~/api/Posts/{id:long}")]
		[ResponseType(typeof(PostModel))]
		public async Task<IHttpActionResult> DeletePost(long id)
		{
			PostModel postModel = await db.Posts.FindAsync(id);
			if (postModel == null)
			{
				return NotFound();
			}

			db.Posts.Remove(postModel);
			await db.SaveChangesAsync();

			return Ok(postModel);
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
