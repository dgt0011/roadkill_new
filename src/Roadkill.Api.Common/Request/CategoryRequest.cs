using System.ComponentModel.DataAnnotations;

namespace Roadkill.Api.Common.Request
{
	/// <summary>
	/// Represents category information in Roadkill.
	/// </summary>
	public class CategoryRequest
	{
		/// <summary>
		/// The unique Id of the category. This is generated on the server.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The title of the category.
		/// </summary>
		[Required]
		public string Title { get; set; }

		/// <summary>
		/// An optional description for the category
		/// </summary>
		public string Description { get; set; }
	}
}
