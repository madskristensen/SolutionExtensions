using System.Collections.Generic;

namespace SolutionExtensions
{
    internal class SuggestionResult
    {
        public IEnumerable<SuggestionModel> Extensions { get; set; }

        public IEnumerable<string> Matches { get; set; }
    }
}
