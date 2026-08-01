using Application.UseCases.NotesUseCases.CreateNote.ModelsNamespace;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class CreateNoteRequestModelExample : IExamplesProvider<CreateNoteRequestModel>
    {
        public CreateNoteRequestModel GetExamples()
        {
            return new CreateNoteRequestModel(
                Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930"),
                "This is a sample note attached to an expense.");
        }
    }
}
