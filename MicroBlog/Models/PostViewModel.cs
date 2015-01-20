using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroBlog.Models
{
	public class PostViewModel
	{
		public long Id { get; set; }
		public string Text { get; set; }
		public UserViewModel Author { get; set; }
		public DateTime CreationTime { get; set; }

		public static PostViewModel FromPost(PostModel postModel)
		{
			var vm = new PostViewModel
			{
				Id = postModel.Id,
				Text = postModel.Text,
				CreationTime = postModel.CreationTime
			};
			if (postModel.Author != null)
				vm.Author = UserViewModel.FromUser(postModel.Author);
			return vm;
		}
	}
}