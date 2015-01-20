using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace MicroBlog.Models
{
	[Table("User")]
	public class UserModel
	{
		[Key]
		public int Id { get; set; }

		[Index("IX_Name_User", IsUnique = true, IsClustered = false)]
		[StringLength(100, ErrorMessage = "User {0} must be have at least {2} and no more then {1} characters", MinimumLength = 3)]
		[Required]
		public string Name { get; set; }
		
		public ICollection<UserModel> Follows { get; set; }
		public ICollection<UserModel> Followers { get; set; }

		public ICollection<PostModel> Posts { get; set; } 

		public UserModel()
		{
			this.Follows = new HashSet<UserModel>();
			this.Followers = new HashSet<UserModel>();
			this.Posts = new HashSet<PostModel>();
		}
	}
}