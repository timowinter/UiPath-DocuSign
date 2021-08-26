using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Model;
using System.IO;

namespace DocumentSign.Models
{
    /// <summary>
    /// Static Class to Create DocuSign Documents
    /// </summary>
    public static class Templater
    {
        /// <summary>
        /// This method create document from byte array template
        /// </summary>
        /// <param name="documentId">document id</param>
        /// <param name="fileName">file name</param>
        /// <param name="fileExtension">file extention</param>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static Document CreateDocumentFromTemplate(String documentId, String fileName, String fileExtension, String filePath)
        {
            Document document = new Document();

            String base64Content = Convert.ToBase64String(ReadContent(filePath));

            document.DocumentBase64 = base64Content;
            // can be different from actual file name
            document.Name = fileName;
            // Source data format. Signed docs are always pdf.
            document.FileExtension = fileExtension;
            // a label used to reference the doc
            document.DocumentId = documentId;

            return document;
        }

        /// <summary>
        /// This method read bytes content from the project's Resources directory
        /// </summary>
        /// <param name="fileName">resource path</param>
        /// <returns>return bytes array</returns>
        internal static byte[] ReadContent(string fileName)
        {
            byte[] buff = null;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    long numBytes = new FileInfo(path).Length;
                    buff = br.ReadBytes((int)numBytes);
                }
            }

            return buff;
        }
    }
}
