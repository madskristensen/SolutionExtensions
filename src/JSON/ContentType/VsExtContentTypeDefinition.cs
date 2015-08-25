using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace SolutionExtensions.JSON.ContentType
{
    class VsextContentTypeDefinition
    {
        public const string VsExtContentType = "vsext";

        [Export(typeof(ContentTypeDefinition))]
        [Name(VsExtContentType)]
        [BaseDefinition("json")]
        public ContentTypeDefinition ICmdContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(VsExtContentType)]
        [FileExtension(".vsext")]
        public FileExtensionToContentTypeDefinition VsextFileExtension { get; set; }
    }
}
