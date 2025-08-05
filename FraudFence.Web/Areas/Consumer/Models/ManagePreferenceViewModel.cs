namespace FraudFence.Web.Areas.Consumer.Models;

public class ManagePreferenceViewModel
{
    public int ScamCategoryId { get; set; }

    public string ScamCategoryName { get; set; } = "";

    public bool Subscribed { get; set; } = false;
}