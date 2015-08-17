namespace SolutionExtensions
{
    public class ExtensionFileEventArgs
    {
        public ExtensionFileModel Model { get; private set; }

        public ExtensionFileEventArgs(ExtensionFileModel model)
        {
            Model = model;
        }
    }
}