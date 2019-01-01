using System;
using System.IO;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a highlighter which provides common functionality.
	/// </summary>
	public static class Highlighter
	{
		/// <summary>
		/// Creates a new instance of a <see cref="Damdai.Forms.ConfiguredHighlighter"/> class.
		/// </summary>
		/// <param name="fileName">The name of the file which contains information about the highlighter.</param>
		/// <param name="language">The name of the language.</param>
		/// <param name="parser">An <see cref="Damdai.Forms.IParser"/> object.</param>
		/// <returns>An <see cref="Damdai.Forms.ConfiguredHighlighter"/> which can be used to highlight source code.</returns>
		public static ConfiguredHighlighter FromFile(string fileName, string language, IParser parser)
		{
			ConfiguredHighlighter highlighter = new ConfiguredHighlighter(parser);

			XmlDocument document = new XmlDocument();
			document.Load(fileName);

			// Find the language element.
			XmlElement languageElement = (XmlElement)document.DocumentElement.SelectSingleNode(String.Format("//highlighter/languages/language[@name='{0}']", language));
			if (languageElement == null)
				throw new ArgumentException("Specified language does not exist.");

			// Load highlighter settings.
			highlighter.Name = languageElement.Attributes["name"].Value;
			highlighter.FullName = languageElement.Attributes["fullName"].Value;
			
			// Load the tag values.
			foreach (XmlElement tagElement in languageElement["tagValues"].ChildNodes)
			{
				highlighter.TagValues.Add(tagElement.InnerText);
			}

			// Load file extensions.
			foreach (XmlElement fileExtensionElement in languageElement["fileExtensions"].ChildNodes)
			{
				highlighter.FileExtensions.Add(fileExtensionElement.InnerText);
			}

			// Load scanners.
			// NOTE: Should use a more elegant solution some day, which won't break in case you change the order of the scanner elements.
			IScanner scanner, previousScanner = null;
			string childID = null;
			foreach (XmlElement scannerElement in languageElement["scanners"].ChildNodes)
			{
				scanner = Register.Instance.Scanners[scannerElement.Attributes["name"].Value].Create(highlighter.GetTokenizer(), highlighter.GetScannerResult());
				scanner.LoadState(scannerElement);
				if (childID != null && scannerElement.Attributes["id"].Value == childID)
					previousScanner.Child = scanner;

				if (Convert.ToBoolean(scannerElement.Attributes["isEntryScanner"].Value))
					highlighter.Scanners.Add(scanner);

				if (scannerElement.Attributes["childScannerID"] != null)
					childID = scannerElement.Attributes["childScannerID"].Value;
				previousScanner = scanner;
			}

			return highlighter;
		}

		/// <summary>
		/// Saves a highlighter's configuration in the specified file.
		/// </summary>
		/// <param name="fileName">The name of the file which will store the information about the highlighter.</param>
		/// <param name="highlighter">An <see cref="Damdai.Forms.HighlighterBase"/> object.</param>
		public static void ToFile(string fileName, HighlighterBase highlighter)
		{
			XmlDocument document = new XmlDocument();

			XmlElement languageRootElement, languageElement;
			if (File.Exists(fileName))
			{
				document.Load(fileName);

				languageRootElement = (XmlElement)document.SelectSingleNode("//highlighter/languages");
				if (languageRootElement != null)
				{
					languageElement = (XmlElement)document.SelectSingleNode(String.Format("//highlighter/languages/language[@name='{0}']", highlighter.Name));
					if (languageElement != null)
						languageRootElement.RemoveChild(languageElement);
				}
			}
			else
			{
				document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", "yes"));

				XmlElement rootElement = document.CreateElement("highlighter");
				document.AppendChild(rootElement);

				languageRootElement = document.CreateElement("languages");
				rootElement.AppendChild(languageRootElement);
			}

			// Store highlighter settings.
			languageElement = document.CreateElement("language");
			languageRootElement.AppendChild(languageElement);
			languageElement.SetAttribute("name", highlighter.Name);
			languageElement.SetAttribute("fullName", highlighter.FullName);

			XmlElement tagRootElement = document.CreateElement("tagValues");
			languageElement.AppendChild(tagRootElement);
			XmlElement tagElement;
			foreach (string tag in highlighter.TagValues)
			{
				tagElement = document.CreateElement("tag");
				tagElement.InnerText = tag;
				tagRootElement.AppendChild(tagElement);
			}

			XmlElement fileExtensionRootElement = document.CreateElement("fileExtensions");
			languageElement.AppendChild(fileExtensionRootElement);
			XmlElement fileExtensionElement;
			foreach (string fileExtension in highlighter.FileExtensions)
			{
				fileExtensionElement = document.CreateElement("tag");
				fileExtensionElement.InnerText = fileExtension;
				fileExtensionRootElement.AppendChild(fileExtensionElement);
			}

			// Store scanner settings.
			XmlElement scannerRootElement = document.CreateElement("scanners");
			languageElement.AppendChild(scannerRootElement);
			int scannerCounter = 0;
			foreach (IScanner scanner in highlighter.Scanners)
			{
				SaveScanner(document, scannerRootElement, scanner, scannerCounter++, true);
			}

			document.Save(fileName);
		}

		/// <summary>
		/// Saves a chain of scanners recursively.
		/// </summary>
		/// <param name="document">The document in which the state will be saved.</param>
		/// <param name="element">The <see cref="System.Xml.XmlElement"/> which will hold the chain of scanners.</param>
		/// <param name="scanner">The scanner which should be stored.</param>
		/// <param name="id">The unique ID of the to be saved scanner.</param>
		/// <param name="isEntryScanner">Whether the scanner is the entry point of the chain.</param>
		/// <returns>The ID of the saved scanner.</returns>
		private static int SaveScanner(XmlDocument document, XmlElement element, IScanner scanner, int id, bool isEntryScanner)
		{
			XmlElement childElement = (XmlElement)scanner.SaveState(document);
			childElement.SetAttribute("id", id.ToString());
			childElement.SetAttribute("isEntryScanner", isEntryScanner.ToString());
			
			element.AppendChild(childElement);

			if (scanner.Child != null)
				childElement.SetAttribute("childScannerID", SaveScanner(document, element, scanner.Child, id + 1, false).ToString());

			return id;
		}
	}
}