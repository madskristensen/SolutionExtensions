using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.JSON.Core.Schema;

namespace SolutionExtensions
{
    [Export(typeof(IJSONSchemaSelector))]
    class SchemaSelector : IJSONSchemaSelector
    {
        public event EventHandler AvailableSchemasChanged { add { } remove { } }

        public Task<IEnumerable<string>> GetAvailableSchemasAsync()
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public string GetSchemaFor(string fileLocation)
        {
            string fileName = Path.GetFileName(fileLocation);

            if (Path.GetExtension(fileName).Equals(Constants.EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                return GetSchemaFileName("json\\schema\\extensions-schema.json");
            }
            else if(fileName.Equals(Constants.SUGGESTIONS_FILENAME, StringComparison.OrdinalIgnoreCase))
            {
                return GetSchemaFileName("json\\schema\\suggestions-schema.json");
            }

            return null;
        }

        private static string GetSchemaFileName(string relativePath)
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            string folder = Path.GetDirectoryName(assembly);
            return Path.Combine(folder, relativePath);
        }
    }
}
