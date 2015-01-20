using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroBlog.Models
{
	/// <summary>
	/// Model which represents the user
	/// </summary>
	public class UserViewModel
	{
		/// <summary>
		/// Unique id of the user
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Unique user name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Create <see cref="UserViewModel"/> from <see cref="UserModel"/>
		/// </summary>
		/// <param name="userModel"></param>
		/// <returns></returns>
		public static UserViewModel FromUser(UserModel userModel)
		{
			return new UserViewModel {Id = userModel.Id, Name = userModel.Name};
		}
	}
}