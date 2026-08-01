namespace Application.Models
{
    public sealed record CategoryDetails(
        Guid Id,
        string Code,
        string Name,
        string Description,
        IReadOnlyList<SubCategoryDetails> SubCategories);

        public sealed record SubCategoryDetails(
        Guid Id,
        string Code,
        string Name,
        string Description);
}
