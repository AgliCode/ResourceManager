using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ResourceManager.Services
{
    internal class ResxWriter
    {
        public void SaveTranslationToResx(Dictionary<string, string> translations, string? englishFilePath)
        {
            if (string.IsNullOrEmpty(englishFilePath))
                throw new InvalidOperationException("No path to the RESX file was provided.");

            var document = System.Xml.Linq.XDocument.Load(englishFilePath);

            foreach (var translation in translations)
            {
                var dataElement = document.Root?
                    .Elements("data")
                    .FirstOrDefault(element =>
                        (string?)element.Attribute("name") == translation.Key);

                if (dataElement != null) dataElement.Element("value")!.Value = translation.Value;
                else
                {
                    // Add translation
                    document.Root?.Add(
                    new System.Xml.Linq.XElement("data",
                        new System.Xml.Linq.XAttribute("name", translation.Key),
                        new System.Xml.Linq.XAttribute(System.Xml.Linq.XNamespace.Xml + "space", "preserve"),
                        new System.Xml.Linq.XElement("value", translation.Value)
                        )
                    );
                }

            }


            document.Save(englishFilePath);
        }
    }
}
