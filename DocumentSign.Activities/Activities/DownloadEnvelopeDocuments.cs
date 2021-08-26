using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using DocuSign.eSign.Model;
using DocumentSign.Models;
using DocumentSign.Enums;
using System.ComponentModel;
using DocuSign.eSign.Api;
using System.Collections.Generic;
using System.IO;

namespace DocumentSign.Activities
{
    [LocalizedDisplayName(nameof(Resources.DownloadEnvelopeDocuments_DisplayName))]
    [LocalizedDescription(nameof(Resources.DownloadEnvelopeDocuments_Description))]
    public class DownloadEnvelopeDocuments : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Envelope ID
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DownloadEnvelopeDocuments_EnvelopeID_DisplayName))]
        [LocalizedDescription(nameof(Resources.DownloadEnvelopeDocuments_EnvelopeID_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> EnvelopeID { get; set; }

        /// <summary>
        /// Folder where the Documents will be saved to
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DownloadEnvelopeDocuments_DownloadPath_DisplayName))]
        [LocalizedDescription(nameof(Resources.DownloadEnvelopeDocuments_DownloadPath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> DownloadPath { get; set; }

        /// <summary>
        /// Folder where the Documents will be saved to
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DownloadEnvelopeDocuments_Files_DisplayName))]
        [LocalizedDescription(nameof(Resources.DownloadEnvelopeDocuments_Files_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> Files { get; set; }

        #endregion


        #region Constructors

        public DownloadEnvelopeDocuments()
        {
            Constraints.Add(ActivityConstraints.HasParentType<DownloadEnvelopeDocuments, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (EnvelopeID == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(EnvelopeID)));
            if (DownloadPath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(DownloadPath)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(SignScope.ParentContainerPropertyTag);

            //Retrieve Authentication Object from Scope by Type
            Authentication authentication = objectContainer.Get<Authentication>();

            // Inputs
            var envelopeID = EnvelopeID.Get(context);
            var downloadPath = DownloadPath.Get(context);

            //authenticate
            authentication.CheckToken();

            //Output object
            List<string> files = new List<string>();

            //instanciate Api
            EnvelopesApi envelopeApi = new EnvelopesApi(authentication.ApiClient.Configuration);

            //list all Documents to given Envelope
            EnvelopeDocumentsResult documents = envelopeApi.ListDocuments(authentication.AccountID, envelopeID);

            // Look up the documents from the list of documents 
            // Process results. Determine the file name and mimetype
            foreach (var docItem in documents.EnvelopeDocuments)
            {
                // get file content
                System.IO.Stream results = envelopeApi.GetDocument(authentication.AccountID, envelopeID, docItem.DocumentId);

                string docName = docItem.Name;

                bool hasPDFsuffix = docName.ToUpper().EndsWith(".PDF");

                bool pdfFile = hasPDFsuffix;

                // Add .pdf if it's a content or summary doc and doesn't already end in .pdf
                string docType = docItem.Type;

                if (("content".Equals(docType) || "summary".Equals(docType)) && !hasPDFsuffix)
                {
                    docName += ".pdf";
                }

                // Add .zip as appropriate
                if ("zip".Equals(docType))
                {
                    docName += ".zip";
                }

                //Save file to given Path
                using (Stream file = File.Create(Path.Combine(downloadPath, docName)))
                {
                    CopyStream(results, file);
                    files.Add(Path.Combine(downloadPath, docName));
                }
            }

            // Outputs
            return (ctx) =>
            {
                Files.Set(ctx, files);
            };
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
        #endregion
    }
}

