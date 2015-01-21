using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Description;
using Antlr.Runtime;
using Castle.Windsor.Diagnostics.Extensions;
using MicroBlog.Models;
using Microsoft.AspNet.Identity;

namespace MicroBlog.Controllers
{
	/// <summary>
	/// Controller to operate on users
	/// </summary>
	[RoutePrefix("api/Users")]
	public class UsersController : ApiController
    {
        private BlogContext db = new BlogContext();

#region Users
        // GET: api/Users
		/// <summary>
		/// Gets the list of all users
		/// </summary>
		/// <returns>The list of users</returns>
		[Route("")]
        public IQueryable<UserViewModel> GetUsers()
        {
            return db.Users.OrderBy(u => u.Name).Select(user => new UserViewModel()
            {
	            Id = user.Id,
				Name = user.Name
            });
        }


		// GET: api/Users/5
		/// <summary>
		/// Gets user details
		/// </summary>
		/// <param name="id">User id to get details for</param>
		/// <returns>User detaile</returns>
		[Route("{id:int}")]
        [ResponseType(typeof(UserDetailViewModel))]
		public async Task<IHttpActionResult> GetUser(int id)
        {
	        UserModel user = await db.Users.FindAsync(id);

	        if (user == null)
		        return NotFound();

			UserDetailViewModel userDetail = new UserDetailViewModel
			{
				Id = user.Id,
				Name = user.Name,
				Follows = user.Follows.Select(UserViewModel.FromUser).ToList()
			};
			return Ok(userDetail);
            return Ok(user);
        }

		// GET: api/Users/abc
		/// <summary>
		/// Gets user details by name
		/// </summary>
		/// <param name="name">User name to get details for</param>
		/// <returns>User instance</returns>
		[ResponseType(typeof(UserDetailViewModel))]
		[Route("{name}")]
		public async Task<IHttpActionResult> GetUser(string name)
		{
			UserModel user = await db.Users.Include(u => u.Follows).FirstOrDefaultAsync(u => u.Name == name);
			
			if (user == null)
			{
				return NotFound();
			}
			UserDetailViewModel userDetail = new UserDetailViewModel
			{
				Id = user.Id,
				Name = user.Name,
				Follows = user.Follows.Select(UserViewModel.FromUser).ToList()
			};
			return Ok(userDetail);
		}
		
        // POST: api/Users
		/// <summary>
		/// Adds new user
		/// </summary>
		/// <param name="user">User to be added</param>
		/// <returns>User added</returns>
		[Route("")]
        [ResponseType(typeof(UserViewModel))]
		public async Task<IHttpActionResult> PostUser(UserViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
	        var newUser = db.Users.Add(new UserModel(){Name = user.Name});
	        try
	        {
		        await db.SaveChangesAsync();
	        }
	        catch (DbEntityValidationException ex)
	        {
		        return
			        BadRequest(string.Join("\r\n",
				        ex.EntityValidationErrors.Where(e => !e.IsValid)
					        .SelectMany(e => e.ValidationErrors)
					        .Select(e => e.ErrorMessage)));
	        }
	        catch (DbUpdateException e)
	        {
		        if (e.InnerException is UpdateException && e.InnerException.InnerException is SqlException)
		        {
			        throw new DuplicateNameException("User with the same name already exists", e);
		        }
		        throw;
	        }
			var res = CreatedAtRoute("", new { controller = "Users", id = newUser.Id }, UserViewModel.FromUser(newUser));
			return res;
        }

        // DELETE: api/Users/5
		/// <summary>
		/// Delete the user with the specified id
		/// </summary>
		/// <param name="id">Id of the user to delete</param>
		/// <returns>User deleted</returns>
        [ResponseType(typeof(UserViewModel))]
		[Route("{id:int}")]
		public async Task<IHttpActionResult> DeleteUser(int id)
        {
            UserModel userModel = await db.Users.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }

            db.Users.Remove(userModel);
            await db.SaveChangesAsync();

            return Ok(UserViewModel.FromUser(userModel));
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

        private bool UserExists(int id)
        {
	        return db.Users.Find(id) != null;
        }
    }
}