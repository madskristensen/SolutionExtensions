using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.JSON.Core.Parser.TreeItems;
using Microsoft.JSON.Editor.Completion;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace SolutionExtensions
{
    /// <summary>
    /// Provides Intellisense for the "license" property of package.json and bower.json
    /// </summary>
    [Export(typeof(IJSONCompletionListProvider))]
    [Name("ProductIdCompletionProvider")]
    internal class LicenseCompletionProvider : IJSONCompletionListProvider
    {
        private static string[] _supported = new string[] { "productid", "name", "description" };

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public JSONCompletionContextType ContextType
        {
            get { return JSONCompletionContextType.PropertyValue; }
        }

        public IEnumerable<JSONCompletionEntry> GetListEntries(JSONCompletionContext context)
        {
            ITextDocument document;
            if (TextDocumentFactoryService.TryGetTextDocument(context.Snapshot.TextBuffer, out document))
            {
                string fileName = Path.GetFileName(document.FilePath).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileName) || (Path.GetExtension(fileName) != Constants.EXTENSION && fileName != Constants.SUGGESTIONS_FILENAME))
                    yield break;
            }
            else
            {
                yield break;
            }

            JSONMember member = context.ContextItem as JSONMember;

            if (member == null || member.Name == null)
                yield break;

            string property = member.UnquotedNameText.ToLowerInvariant();

            if (!_supported.Contains(property))
                yield break;

            foreach (var extension in ExtensionInstalledChecker.Instance.GetInstalledExtensions())
            {
                ImageSource glyph = GetExtensionIcon(extension);

                if (property == "productid")
                {
                    yield return new SimpleCompletionEntry(extension.Header.Name, extension.Header.Identifier, glyph, context.Session);
                }
                else if (property == "name")
                {
                    yield return new SimpleCompletionEntry(extension.Header.Name, extension.Header.Name, glyph, context.Session);
                }
                else if (property == "description")
                {
                    yield return new SimpleCompletionEntry(extension.Header.Name, extension.Header.Description, glyph, context.Session);
                }
            }
        }

        private static ImageSource GetExtensionIcon(IInstalledExtension extension)
        {
            try
            {
                var prop = extension.GetType().GetProperty("IconFullPath", BindingFlags.Public | BindingFlags.Instance);

                if (prop != null)
                {
                    string icon = prop.GetValue(extension) as string;

                    if (!string.IsNullOrEmpty(icon) && File.Exists(icon))
                        return BitmapFrame.Create(new Uri(icon, UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return null;
        }
    }
}