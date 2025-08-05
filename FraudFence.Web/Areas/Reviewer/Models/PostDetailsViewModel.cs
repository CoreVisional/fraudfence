using System;
using System.Collections.Generic;
using FraudFence.EntityModels.Enums;

namespace FraudFence.Web.Areas.Reviewer.Models
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<string> ImageLinks { get; set; } = new List<string>();
        public PostStatus Status { get; set; }
    }
} 