namespace Application.Models
{
    public record PaginationParameters
    {
        public int PageNumber { get; init; } = PaginationParametersConstants.DefaultPageNumber;
        public int PageSize { get; init; } = PaginationParametersConstants.DefaultPageSize;
    }
}
