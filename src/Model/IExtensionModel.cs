using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionExtensions
{
    public interface IExtensionModel
    {
        string Name { get; }

        string Description { get; }

        string ProductId { get; }

        string Category { get; }
    }
}
