using Domain.Entities.DomainEnums;

namespace Application.Models
{
    public sealed record UserClaims(Guid Id, Role Role);
}
