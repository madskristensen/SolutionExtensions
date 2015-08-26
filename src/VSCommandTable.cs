namespace SolutionExtensions
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidVSPackageString = "b8f5f6a7-06c9-4303-9f98-f80c74c26614";
        public const string guidExtensionCmdSetString = "b66e17f2-a296-460f-8f86-021c91ccdc5d";
        public static Guid guidVSPackage = new Guid(guidVSPackageString);
        public static Guid guidExtensionCmdSet = new Guid(guidExtensionCmdSetString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int MyMenu = 0x1010;
        public const int MyMenuGroup = 0x1020;
        public const int MissingGroup = 0x1030;
        public const int SuggestionGroup = 0x1040;
        public const int cmdShowMissing = 0x0100;
        public const int cmdShowSuggestions = 0x0200;
        public const int cmdCreateSolutionExtensions = 0x0300;
    }
}
