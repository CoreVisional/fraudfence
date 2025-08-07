namespace FraudFence.EntityModels.Dto.Article
{
    public record CreateArticleDTO(string Title, string Content, int ScamCategoryId, string UserId);
}
