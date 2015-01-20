using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroBlog.Models
{
	public class UserDetailViewModel : UserViewModel
	{
		public List<UserViewModel> Follows { get; set; } 
	}
}