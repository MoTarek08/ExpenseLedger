using Domain.Entities.DomainEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference.Models
{
    public sealed record CreateCategoryPreferenceRequestModel(
        Guid CategoryId,
        CategoryPreferenceLevel PreferenceLevel);
}
