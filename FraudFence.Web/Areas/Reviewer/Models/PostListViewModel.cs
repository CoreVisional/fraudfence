using System;

namespace FraudFence.Web.Areas.Reviewer.Models
{
    public class PostListViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 