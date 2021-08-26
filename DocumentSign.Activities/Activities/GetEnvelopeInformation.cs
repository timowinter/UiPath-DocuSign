using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
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
    [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_DisplayName))]
    [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_Description))]
    public class GetEnvelopeInformation : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_EnvelopeId_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_EnvelopeId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> EnvelopeId { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_DocumentIds_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_DocumentIds_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> DocumentIds { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_Recpients_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_Recpients_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<KeyValuePair<string, string>>> Recipients { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_EnvelopeStatus_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_EnvelopeStatus_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> EnvelopeStatus { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_StatusChangedDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_StatusChangedDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> StatusChangedDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_CreatedDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_CreatedDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> CreatedDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_DeclinedDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_DeclinedDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> DeclinedDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_DeletedDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_DeletedDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> DeletedDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_DeliveredDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_DeliveredDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> DeliveredDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_ExpireDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_ExpireDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> ExpireDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_SentDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_SentDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> SentDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_VoidedDateTime_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_VoidedDateTime_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> VoidedDateTime { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_VoidedReason_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_VoidedReason_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> VoidedReason { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetEnvelopeInformation_JsonData_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetEnvelopeInformation_JsonData_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> JsonData { get; set; }

        #endregion


        #region Constructors

        public GetEnvelopeInformation()
        {
            Constraints.Add(ActivityConstraints.HasParentType<GetEnvelopeInformation, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (EnvelopeId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(EnvelopeId)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(SignScope.ParentContainerPropertyTag);

            //Retrieve Authentication Object from Scope by Type
            Authentication authentication = objectContainer.Get<Authentication>();

            //instanciate Api
            EnvelopesApi envelopeApi = new EnvelopesApi(authentication.ApiClient.Configuration);

            //authenticate
            authentication.CheckToken();

            // Inputs
            var envelopeid = EnvelopeId.Get(context);

            //Get Envelope Information for EnvelopeId
            Envelope envelope = envelopeApi.GetEnvelope(authentication.AccountID, envelopeid);

            //Get all docuemnt ids
            var docs = envelopeApi.ListDocuments(authentication.AccountID, envelopeid);
            
            List<string> documentIds = new List<string>();
            foreach (var item in docs.EnvelopeDocuments)
            {
                documentIds.Add(item.DocumentId);
            }


            //Loop through all recipients
            var recipients = envelopeApi.ListRecipients(authentication.AccountID, envelopeid);

            List<KeyValuePair<string, string>> recipientList = new List<KeyValuePair<string, string>>();
            foreach (var item in recipients.Agents)
            {
                recipientList.Add(new KeyValuePair<string, string>("Agents", item.Email));
            }
            foreach (var item in recipients.CarbonCopies)
            {
                recipientList.Add(new KeyValuePair<string, string>("CarbonCopy", item.Email));
            }
            foreach (var item in recipients.Signers)
            {
                recipientList.Add(new KeyValuePair<string, string>("Signer", item.Email));
            }
            foreach (var item in recipients.Editors)
            {
                recipientList.Add(new KeyValuePair<string, string>("Editor", item.Email));
            }
            foreach (var item in recipients.InPersonSigners)
            {
                recipientList.Add(new KeyValuePair<string, string>("InPersonSigners", item.Email));
            }
            foreach (var item in recipients.Intermediaries)
            {
                recipientList.Add(new KeyValuePair<string, string>("Intermediaries", item.Email));
            }
            foreach (var item in recipients.Witnesses)
            {
                recipientList.Add(new KeyValuePair<string, string>("Witnesses", item.Email));
            }

            // Outputs
            return (ctx) =>
            {
                DocumentIds.Set(ctx, documentIds);
                Recipients.Set(ctx, recipientList);
                EnvelopeStatus.Set(ctx, envelope.Status);
                StatusChangedDateTime.Set(ctx, envelope.StatusChangedDateTime);
                CreatedDateTime.Set(ctx, envelope.CreatedDateTime);
                DeclinedDateTime.Set(ctx, envelope.DeclinedDateTime);
                DeletedDateTime.Set(ctx, envelope.DeletedDateTime);
                DeliveredDateTime.Set(ctx, envelope.DeliveredDateTime);
                ExpireDateTime.Set(ctx, envelope.ExpireDateTime);
                SentDateTime.Set(ctx, envelope.SentDateTime);
                VoidedDateTime.Set(ctx, envelope.VoidedDateTime);
                VoidedReason.Set(ctx, envelope.VoidedReason);
                JsonData.Set(ctx, envelope.ToJson());
            };
        }

        #endregion
    }
}

