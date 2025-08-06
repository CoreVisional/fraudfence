namespace FraudFence.EntityModels.Dto.Article
{
    public record ArticleDTO(int Id, string Title, string CategoryName, string Content, int ScamCategoryId, DateTime CreatedAt, DateTime? LastModified);
}
