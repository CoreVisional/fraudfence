using System.ComponentModel.DataAnnotations;
using FraudFence.EntityModels.Models;

namespace FraudFence.Web.Areas.Consumer.Models;

public class CommentViewModel
{
    public int PostId { get; set; }

    [Required(ErrorMessage = "Comment content is required.")]
    [MinLength(1, ErrorMessage = "Comment cannot be empty.")]
    public string Content { get; set; } = string.Empty;

    public Post? Post { get; set; }
}