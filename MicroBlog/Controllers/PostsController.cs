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
	[RoutePrefix("api/Users/{userId:int}")]
	public class PostsController : ApiController
    {
		private BlogContext db = new BlogContext();

		#region User Posts
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
