using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using DocuSign.eSign.Model;
using System.Linq;
using System.Collections.Generic;
using DocumentSign.Models;

namespace DocumentSign.Activities
{
    [LocalizedDisplayName(nameof(Resources.DefineEnvelope_DisplayName))]
    [LocalizedDescription(nameof(Resources.DefineEnvelope_Description))]
    public class DefineEnvelope : ContinuableAsyncCodeActivity
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
        /// List of Documents which are generated through Create Document Activity
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DefineEnvelope_Documents_DisplayName))]
        [LocalizedDescription(nameof(Resources.DefineEnvelope_Documents_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<IEnumerable<object>> Documents { get; set; }

        /// <summary>
        /// List of Recipients which are generated through Create Recipient Activity e.g. Signers, CarbonCopies etc.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DefineEnvelope_Recipients_DisplayName))]
        [LocalizedDescription(nameof(Resources.DefineEnvelope_Recipients_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<IEnumerable<object>> Recipients { get; set; }

        /// <summary>
        /// Envelope Document
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.DefineEnvelope_Envelope_DisplayName))]
        [LocalizedDescription(nameof(Resources.DefineEnvelope_Envelope_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public InOutArgument<object> Envelope { get; set; }

        #endregion


        #region Constructors

        public DefineEnvelope()
        {
            Constraints.Add(ActivityConstraints.HasParentType<DefineEnvelope, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));
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

            // Inputs
            var documents = Documents.Get(context);
            var assignees = Recipients.Get(context);
            var envelopedefinition = Envelope.Get(context);

            
            EnvelopeDefinition envelope = new EnvelopeDefinition();
            if (envelopedefinition != null && envelopedefinition.GetType() == typeof(EnvelopeDefinition))
                envelope = (EnvelopeDefinition)envelopedefinition;

            //
            Recipients recipients = new Recipients();
            if (envelope.Recipients != null)
                recipients = envelope.Recipients;

            //Loop through all Documents
            foreach (var singleDocument in documents)
            {

                //Only Documents of Type TransportDocument are valid here
                if (singleDocument != null && singleDocument.GetType() == typeof(Document))
                {
                    //Add the Documents to Envelope
                    if ((envelope.Documents != null) && (envelope.Documents.Any()) && !envelope.Documents.Contains((Document)singleDocument))
                        envelope.Documents.Add(((Document)singleDocument));
                    else
                        envelope.Documents = new List<Document>() { ((Document)singleDocument) };
                }

                //Loop through all Recipients which are provided and add them to the Envelope
                foreach (var assignee in assignees)
                {
                    switch (assignee.GetType().Name)
                    {
                        case "Agent":                         
                            if (recipients.Agents != null && !recipients.Agents.Contains((Agent)assignee))
                                recipients.Agents.Add((Agent)assignee);
                            else
                                recipients.Agents = new List<Agent>() { (Agent)assignee };
                            break;
                        case "Editor":                   
                            if (recipients.Editors != null && !recipients.Editors.Contains((Editor)assignee))
                                recipients.Editors.Add((Editor)assignee);
                            else
                                recipients.Editors = new List<Editor>() { (Editor)assignee };
                            break;
                        case "Signer":                      
                            //Signers will also get the Signature Tab from our TransportDocument Object which belongs to the added Document
                            Signer signer = (Signer)assignee;  

                            //Add finally to envelope
                            if (recipients.Signers != null && !recipients.Signers.Contains((Signer)assignee))
                                recipients.Signers.Add(signer);
                            else
                                recipients.Signers = new List<Signer>() { signer };
                            break;
                        case "CarbonCopy":                            
                            if (recipients.CarbonCopies != null && !recipients.CarbonCopies.Contains((CarbonCopy)assignee))
                                recipients.CarbonCopies.Add((CarbonCopy)assignee);
                            else
                                recipients.CarbonCopies = new List<CarbonCopy>() { (CarbonCopy)assignee };
                            break;
                        default:
                            break;
                    }
                }
            }

            //Finally add Recipients to our Envelope Object
            envelope.Recipients = recipients;            

            // Outputs
            return (ctx) =>
            {
                Envelope.Set(ctx, envelope);
            };
        }

        #endregion
    }
}

