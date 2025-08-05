namespace FraudFence.EntityModels.Dto
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
} 