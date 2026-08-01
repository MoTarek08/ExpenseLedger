using Application.UseCases.NotesUseCases.UpdateNote.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class UpdateNoteRequestModelExample : IExamplesProvider<UpdateNoteRequestModel>
    {
        public UpdateNoteRequestModel GetExamples()
        {
            return new UpdateNoteRequestModel("Updated note content.");
        }
    }
}
