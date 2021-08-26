using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using DocuSign.eSign.Model;
using DocumentSign.Enums;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocumentSign.Activities
{
    /// <summary>
    /// Method to Create Recipient from Details
    /// </summary>
    [LocalizedDisplayName(nameof(Resources.CreateRecipient_DisplayName))]
    [LocalizedDescription(nameof(Resources.CreateRecipient_Description))]
    public class CreateRecipient : ContinuableAsyncCodeActivity
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
        /// Role of Recipient 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_Role_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_Role_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public Role Role { get; set; }

        /// <summary>
        /// Email Address of Recipient
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_EmailAddress_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_EmailAddress_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> EmailAddress { get; set; }

        /// <summary>
        /// Full legal name of Recipient
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_FullName_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_FullName_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FullName { get; set; }

        /// <summary>
        /// Routing Order -> Low Number is High Priority
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_RoutingOrder_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_RoutingOrder_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<int> RoutingOrder { get; set; }

        /// <summary>
        /// Routing Order -> Low Number is High Priority
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_DocumentId_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_DocumentId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> DocumentId { get; set; }

        /// <summary>
        /// AchorText for Signature Field  
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_SignatureAnchor_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_SignatureAnchor_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> SignatureAnchor { get; set; }

        /// <summary>
        /// Anchor Option 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_AnchorOption_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_AnchorOption_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public AnchorOption AnchorOption { get; set; }

        /// <summary>
        /// X Offset for Signature Placement
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_XOffset_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_XOffset_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<int> XOffset { get; set; }

        /// <summary>
        /// Y Offset for Signature Placement
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateDocument_YOffset_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateDocument_YOffset_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<int> YOffset { get; set; }


        /// <summary>
        /// Recipient Object
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.CreateRecipient_Recipient_DisplayName))]
        [LocalizedDescription(nameof(Resources.CreateRecipient_Recipient_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<object> Recipient { get; set; }

        #endregion


        #region Constructors

        public CreateRecipient()
        {
            Constraints.Add(ActivityConstraints.HasParentType<CreateRecipient, SignScope>(string.Format(Resources.ValidationScope_Error, Resources.SignScope_DisplayName)));

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (EmailAddress == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error,nameof(EmailAddress)));

            base.CacheMetadata(metadata);
        }

    

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(SignScope.ParentContainerPropertyTag);

            // Inputs
            var emailaddress = EmailAddress.Get(context);
            var fullname = FullName.Get(context);
            var routingOrder = RoutingOrder.Get(context);
            var documentId = DocumentId.Get(context);
            var signatureanchor = SignatureAnchor.Get(context);
            var xoffset = XOffset.Get(context);
            var yoffset = YOffset.Get(context);

            Regex regex = new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            bool isValid = regex.IsMatch(emailaddress);

            if (!isValid)
                throw new Exception("Invalid Email Address provided.");

            object recipient = null;
            switch (Role)
            {
                case Enums.Role.Signer:
                    Signer _signer = new Signer
                    {
                        Email = emailaddress,
                        Name = fullname,
                        RecipientId = Guid.NewGuid().ToString(),
                        RoutingOrder = routingOrder.ToString()
                    };

                    _signer.Tabs = new Tabs();                   
                    _signer.Tabs.SignHereTabs = new List<SignHere>();

                    SignHere signHere = new SignHere();
                    signHere.DocumentId = documentId;
                    signHere.RecipientId = _signer.RecipientId;

                    //Define location of Signature Tab in Document
                    if (!String.IsNullOrEmpty(signatureanchor))
                    {
                        signHere.AnchorString = signatureanchor;
                        signHere.AnchorAllowWhiteSpaceInCharacters = "true";

                        if (AnchorOption == Enums.AnchorOption.IgnoreAnchorIfNotPresent)
                            signHere.AnchorIgnoreIfNotPresent = "true";

                        if (AnchorOption == Enums.AnchorOption.EnforceAnchor)
                            signHere.AnchorIgnoreIfNotPresent = "false";

                        signHere.AnchorXOffset = xoffset.ToString();
                        signHere.AnchorYOffset = yoffset.ToString();
                    }

                    _signer.Tabs.SignHereTabs.Add(signHere);

                    recipient = _signer;
                    break;
                case Enums.Role.CarbonCopy:
                    CarbonCopy _cc = new CarbonCopy
                    {
                        Email = emailaddress,
                        Name = fullname,
                        RecipientId = Guid.NewGuid().ToString(),
                        RoutingOrder = routingOrder.ToString()
                    };
                    recipient = _cc;
                    break;
                case Enums.Role.Agents:
                    Agent _agent = new Agent
                    {
                        Email = emailaddress,
                        Name = fullname,
                        RecipientId = Guid.NewGuid().ToString(),
                        RoutingOrder = routingOrder.ToString()
                    };
                    recipient = _agent;
                    break;
                case Enums.Role.Editor:
                    Editor _editor = new Editor
                    {
                        Email = emailaddress,
                        Name = fullname,
                        RecipientId = Guid.NewGuid().ToString(),
                        RoutingOrder = routingOrder.ToString()
                    };
                    recipient = _editor;
                    break;
                default:
                    break;
            }

            // Outputs
            return (ctx) =>
            {
                Recipient.Set(ctx, recipient);
            };
        }

        #endregion
    }
}

