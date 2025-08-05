using FraudFence.EntityModels.Models;

namespace FraudFence.Web.Areas.Consumer.Models;

public class EditPostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    
    public Post? Post = null;
}