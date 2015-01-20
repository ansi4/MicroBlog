using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MicroBlog.Models
{
	[Table("Post")]
	public class PostModel
	{
		[Key]
		public long Id { get; set; }
		[Required]
		public UserModel Author { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime CreationTime { get; set; }

		[Required]
		[MaxLength(140)]
		public string Text { get; set; }
	}
}