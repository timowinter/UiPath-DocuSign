using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using DocumentSign.Enums;
using DocuSign.eSign.Model;
using DocumentSign.Models;
using DocuSign.eSign.Api;
using UiPath.Shared.Activities.Utilities;
using static DocuSign.eSign.Api.EnvelopesApi;
using System.Collections.Generic;

namespace DocumentSign.Activities
{
    [LocalizedDisplayName(nameof(Resources.ListEnvelopes_DisplayName))]
    [LocalizedDescription(nameof(Resources.ListEnvelopes_Description))]
    public class ListEnvelopes : ContinuableAsyncCodeActivity
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
        /// Filter for Envelopes which are Created From This Date
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_FromDate_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_FromDate_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FromDate { get; set; }

        /// <summary>
        /// Filter for Envelopes which are Created to This Date
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_ToDate_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_ToDate_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> ToDate { get; set; }

        /// <summary>
        /// Filter for Envelopes which have changed their Status to
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_FromToStatus_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_FromToStatus_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public EffectiveEnvelopeStatus FromToStatus { get; set; }

        /// <summary>
        /// EnvelopeIDs to Look for
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_EnvelopeIDs_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_EnvelopeIDs_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<String[]> EnvelopeIDs { get; set; }

        /// <summary>
        /// Transactions IDs
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_TransactionIDs_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_TransactionIDs_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<String[]> TransactionIDs { get; set; }

        /// <summary>
        /// Limit the maximum Results
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_MaxResults_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_MaxResults_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<int> MaxResults { get; set; }

        /// <summary>
        /// Setup a filter for a Custom Field. Make Syntax correctness sure
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_CustomField_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_CustomField_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> CustomField { get; set; }

        /// <summary>
        /// Free Text filter option
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_SearchText_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_SearchText_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> SearchText { get; set; }

        /// <summary>
        /// The the matched Envelopes (IDs) as an Array
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.ListEnvelopes_MatchingEnvelopes_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListEnvelopes_MatchingEnvelopes_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string[]> MatchingEnvelopes { get; set; }


        #endregion


        #region Constructors

        public ListEnvelopes()
        {
            Constraints.Add(ActivityConstraints.HasParentType<ListEnvelopes, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));
            //includings = 0;

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(SignScope.ParentContainerPropertyTag);

            //Retrieve Authentication Object from Scope by Type
            Authentication authentication = objectContainer.Get<Authentication>();

            // Inputs
            var fromdate = FromDate.Get(context);
            var todate = ToDate.Get(context);
            var envelopeids = EnvelopeIDs.Get(context);
            var transactionids = TransactionIDs.Get(context);
            var maxresults = MaxResults.Get(context);
            var customfield = CustomField.Get(context);
            var searchtext = SearchText.Get(context);

            //authenticate
            authentication.CheckToken();

            //Instanciate Api
            EnvelopesApi envelopeApi = new EnvelopesApi(authentication.ApiClient.Configuration);

            //Bring all Optional Filter parameter to this object
            ListStatusChangesOptions options = new ListStatusChangesOptions();
            if (envelopeids != null)
                options.envelopeIds = String.Join(",", envelopeids);
            if (fromdate != null)
                options.fromDate = fromdate;
            if (todate != null)
                options.toDate = todate;
            if (transactionids != null)
                options.transactionIds = String.Join(",", transactionids);
            if (maxresults > 0)
                options.count = maxresults.ToString();
            if (customfield != null)
                options.customField = customfield;
            if (searchtext != null)
                options.searchText = searchtext;

            options.fromToStatus = FromToStatus.ToString();

            //List all Statu Changes that match to all given Options
            EnvelopesInformation results = envelopeApi.ListStatusChanges(authentication.AccountID, options);

            //Add all EnvelopeIds that match the criteria to the output object
            List<string> envelopeResults = new List<string> { };
            if (results != null && results.Envelopes != null && results.Envelopes.Count > 0)
                results.Envelopes.ForEach(item => envelopeResults.Add(item.EnvelopeId));

            // Outputs
            return (ctx) =>
            {
                MatchingEnvelopes.Set(ctx, envelopeResults.ToArray());
            };
        }

        #endregion
    }
}

