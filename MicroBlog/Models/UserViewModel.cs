using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroBlog.Models
{
	public class UserViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public static UserViewModel FromUser(UserModel userModel)
		{
			return new UserViewModel {Id = userModel.Id, Name = userModel.Name};
		}
	}
}