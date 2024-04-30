using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Roadkill.Core.Entities;

namespace Roadkill.Api.Common.Response
{
	/// <summary>
	/// Represents page information in Roadkill.
	/// </summary>
	[AutoMap(typeof(Category))]
	public class CategoryResponse
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
