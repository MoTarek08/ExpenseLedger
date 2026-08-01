using Host.Attributes;
using Host.ProblemDetailsNamespace.ProblemDefinitionNamespace;
using System.Reflection;

namespace Host.ProblemDetails.Problems
{
    public static class AllProblems
    {
        private static readonly Lazy<IReadOnlyDictionary<string, ProblemDefinition>> _all = new(() =>
        {
            var combined = new Dictionary<string, ProblemDefinition>();

            var problemTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t is { IsClass: true })
                .Where(t => t != typeof(AllProblems))
                .Where(t => t.GetCustomAttribute<ProblemDictionaryAttribute>() != null)
                .Select(t => new
                {
                    Type = t,
                    Field = t.GetField("All", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                })
                .Where(x => x.Field?.FieldType == typeof(IReadOnlyDictionary<string, ProblemDefinition>));

            foreach (var x in problemTypes)
            {
                if (x.Field!.GetValue(null) is IReadOnlyDictionary<string, ProblemDefinition> dict)
                {
                    foreach (var kvp in dict)
                    {
                        combined[kvp.Key] = kvp.Value;
                    }
                }
            }

            return combined;
        });

        public static IReadOnlyDictionary<string, ProblemDefinition> All => _all.Value;

        public static ProblemDefinition Get(string errorCode)
        {
            if (!_all.Value.TryGetValue(errorCode, out var problem))
                throw new InvalidOperationException("Invalid error code");

            return problem;
        }
    }
}
