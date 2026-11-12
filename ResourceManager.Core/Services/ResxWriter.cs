using ResourceManager.Core.Models;

namespace ResourceManager.Core.Services
{
    public class ResxWriter
    {
        public static void SaveTranslationToResx(Dictionary<string, string> translations, string? targetFilePath)
        {
            if (string.IsNullOrEmpty(targetFilePath))
                throw new InvalidOperationException("No path to the RESX file was provided.");

            var document = System.Xml.Linq.XDocument.Load(targetFilePath);

            foreach (var translation in translations)
            {
                var dataElement = document.Root?
                    .Elements("data")
                    .FirstOrDefault(element =>
                        (string?)element.Attribute("name") == translation.Key);

                if (dataElement != null) dataElement.Element("value")!.Value = translation.Value;
                else
                {
                    document.Root?.Add(
                    new System.Xml.Linq.XElement("data",
                        new System.Xml.Linq.XAttribute("name", translation.Key),
                        new System.Xml.Linq.XAttribute(System.Xml.Linq.XNamespace.Xml + "space", "preserve"),
                        new System.Xml.Linq.XElement("value", translation.Value)
                        )
                    );
                }
            }

            document.Save(targetFilePath);
        }
        public static void CreateTranslatedResx(string sourceFilePath,string outputFilePath, IEnumerable<NewLanguageEntry> entries)
        {
            var sourceDocument = System.Xml.Linq.XDocument.Load(sourceFilePath);

            var sourceRoot = sourceDocument.Root
                ?? throw new InvalidOperationException("Invalid RESX file.");

            var newRoot = new System.Xml.Linq.XElement(
                sourceRoot.Name,
                sourceRoot.Attributes(),
                sourceRoot.Elements()
                    .Where(element => element.Name.LocalName != "data")
                    .Select(element => new System.Xml.Linq.XElement(element))
            );

            foreach (var entry in entries)
            {
                var originalData = sourceRoot.Elements()
                    .FirstOrDefault(element =>
                        element.Name.LocalName == "data" &&
                        (string?)element.Attribute("name") == entry.Name);

                if (originalData == null)
                    throw new InvalidOperationException(
                        $"Resource '{entry.Name}' was not found.");

                var translatedData = new System.Xml.Linq.XElement(originalData);
                translatedData.Element("value")!.Value = entry.TranslatedValue ?? string.Empty;

                newRoot.Add(translatedData);
            }

            new System.Xml.Linq.XDocument(
                sourceDocument.Declaration,
                newRoot
            ).Save(outputFilePath);
        }

        public void AddEntryToResx(
            string key,
            string neutralValue,
            string? comment,
            string? translatedValue,
            string? baseFilePath,
            string? targetFilePath)
        {
            if (string.IsNullOrWhiteSpace(baseFilePath))
            {
                throw new InvalidOperationException(
                    "Base resource file path has not been configured.");
            }

            bool hasSeparateTargetFile = !string.IsNullOrWhiteSpace(targetFilePath);

            if (hasSeparateTargetFile && string.IsNullOrWhiteSpace(translatedValue))
            {
                throw new InvalidOperationException(
                    "Target value cannot be empty.");
            }

            var baseDocument = System.Xml.Linq.XDocument.Load(baseFilePath);

            var baseRoot = baseDocument.Root
                ?? throw new InvalidOperationException("Invalid base RESX file.");

            System.Xml.Linq.XDocument? targetDocument = null;
            System.Xml.Linq.XElement? targetRoot = null;

            if (hasSeparateTargetFile)
            {
                targetDocument = System.Xml.Linq.XDocument.Load(targetFilePath!);

                targetRoot = targetDocument.Root
                    ?? throw new InvalidOperationException("Invalid target RESX file.");
            }

            bool keyExists = baseRoot.Elements().Any(element =>
                element.Name.LocalName == "data" &&
                (string?)element.Attribute("name") == key);

            if (targetRoot != null)
            {
                keyExists |= targetRoot.Elements().Any(element =>
                    element.Name.LocalName == "data" &&
                    (string?)element.Attribute("name") == key);
            }

            if (keyExists)
            {
                throw new InvalidOperationException(
                    $"The resource key '{key}' already exists in one of the files.");
            }

            baseRoot.Add(CreateDataElement(
                baseRoot,
                key,
                neutralValue,
                comment));

            if (targetRoot != null)
            {
                targetRoot.Add(CreateDataElement(
                    targetRoot,
                    key,
                    translatedValue!,
                    comment));
            }

            baseDocument.Save(baseFilePath);

            targetDocument?.Save(targetFilePath!);
        }

        private System.Xml.Linq.XElement CreateDataElement(
            System.Xml.Linq.XElement root,
            string key,
            string value,
            string? comment = null)
        {
            var dataElement = new System.Xml.Linq.XElement(
                root.Name.Namespace + "data",
                new System.Xml.Linq.XAttribute("name", key),
                new System.Xml.Linq.XAttribute(
                    System.Xml.Linq.XNamespace.Xml + "space",
                    "preserve"),
                new System.Xml.Linq.XElement(
                    root.Name.Namespace + "value",
                    value));

            if (!string.IsNullOrWhiteSpace(comment))
            {
                dataElement.Add(
                    new System.Xml.Linq.XElement(
                        root.Name.Namespace + "comment",
                        comment));
            }

            return dataElement;
        }
    }
}
