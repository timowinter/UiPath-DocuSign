using DocuSign.eSign.Model;

namespace DocumentSign.Models
{
    /// <summary>
    /// Class to keep Document and related Signature Information at one sport
    /// </summary>
    public class TransportDocument
    {
        /// <summary>
        /// Signing Information
        /// </summary>
        public SignHere SignHere { get; set; }

        /// <summary>
        /// Document Information and Document Content
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_signHere"></param>
        /// <param name="_document"></param>
        public TransportDocument(SignHere _signHere, Document _document)
        {
            this.SignHere = _signHere;
            this.Document = _document;
        }
    }
}
