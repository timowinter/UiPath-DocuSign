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
using System.IO;

namespace DocumentSign.Activities
{
    [LocalizedDisplayName(nameof(Resources.CreateDocument_DisplayName))]
    [LocalizedDescription(nameof(Resources.CreateDocument_Description))]
    public class CreateDocument : ContinuableAsyncCodeActivity
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
        /// File Path to Document
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_DocumentPath_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_DocumentPath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> DocumentPath { get; set; }


        /// <summary>
        /// Resulting Document
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_Document_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_Document_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<object> Document { get; set; }

        /// <summary>
        /// Resulting Document
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_DocumentId_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_DocumentId_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> DocumentId { get; set; }

        #endregion


        #region Constructors

        public CreateDocument()
        {
            Constraints.Add(ActivityConstraints.HasParentType<CreateDocument, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (DocumentPath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(DocumentPath)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(SignScope.ParentContainerPropertyTag);

            // Inputs
            var documentpath = DocumentPath.Get(context);

            Document transportDocument = null;
            string documentId = string.Empty;
            try
            {
                //New Instance of Document
                Document documentDefinition = new Document();
                FileInfo DocumentInfo = new FileInfo(documentpath);

                //Generate a pseudorandom documentId from time = unsigned integer
                documentId = new Random(DateTime.Now.Millisecond).Next().ToString();

                //Create Document 
                if (DocumentInfo.Exists)
                    documentDefinition = Templater.CreateDocumentFromTemplate(documentId, DocumentInfo.Name, DocumentInfo.Extension, DocumentInfo.FullName);
                else
                    throw new FileNotFoundException(DocumentInfo.FullName);

                //Put user data in data structure that contains our Document and its Signature Details
                transportDocument = documentDefinition;

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            // Outputs
            return (ctx) =>
            {
                Document.Set(ctx, transportDocument);
                DocumentId.Set(ctx, documentId);
            };
        }

        #endregion
    }
}

