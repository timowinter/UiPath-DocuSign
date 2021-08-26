using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Activities.Statements;
using System.ComponentModel;
using DocumentSign.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using DocumentSign.Enums;
using DocumentSign.Models;

namespace DocumentSign.Activities
{
    [LocalizedDisplayName(nameof(Resources.SignScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.SignScope_Description))]
    public class SignScope : ContinuableAsyncNativeActivity
    {
        #region Properties

        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Authentication Server (endpoint)
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_AuthenticationServer_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_AuthenticationServer_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        [TypeConverter(typeof(EnumNameConverter<DocuSignSystem>))]
        public DocuSignSystem AuthenticationServer { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_ClientD_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_ClientD_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> ClientD { get; set; }

        /// <summary>
        /// User ID or called 'IntegrationKey'
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_UserID_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_UserID_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> UserID { get; set; }

        /// <summary>
        /// RSA Private Key in PEM Base64 Format
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_PrivateKey_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_PrivateKey_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> PrivateKey { get; set; }

        /// <summary>
        /// Optioinal Redirect Uri
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_RedirectUri_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_RedirectUri_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> RedirectUri { get; set; }

        /// <summary>
        /// Optional TargetAccountID
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.SignScope_TargetAccountID_DisplayName))]
        [LocalizedDescription(nameof(Resources.SignScope_TargetAccountID_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> TargetAccountID { get; set; }

        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        #endregion


        #region Constructors

        public SignScope(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer> (ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public SignScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (ClientD == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientD)));
            if (UserID == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(UserID)));
            if (PrivateKey == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(PrivateKey)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext  context, CancellationToken cancellationToken)
        {
            // Inputs
            var clientid = ClientD.Get(context);
            var userid = UserID.Get(context);
            var privatekey = PrivateKey.Get(context);
            var redirecturi = RedirectUri.Get(context);
            var targetaccountid = TargetAccountID.Get(context);

            string authserver = string.Empty;
            switch (AuthenticationServer)
            {
                case Enums.DocuSignSystem.Demo_OAuth_BasePath:
                    authserver = DocuSign.eSign.Client.Auth.OAuth.Demo_OAuth_BasePath;
                    break;
                case Enums.DocuSignSystem.Production_OAuth_BasePath:
                    authserver = DocuSign.eSign.Client.Auth.OAuth.Production_OAuth_BasePath;
                    break;
                default:
                    authserver = DocuSign.eSign.Client.Auth.OAuth.Demo_OAuth_BasePath;
                    break;
            }

            Authentication senderBase = new Authentication(new DocuSign.eSign.Client.ApiClient(), clientid, userid, authserver, privatekey,targetaccountid, redirecturi);

            senderBase.CheckToken();

            //send sender object to children
            _objectContainer.Add(senderBase);



            return (ctx) => {
                // Schedule child activities
                if (Body != null)
				    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
            };
        }

        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Cleanup();
        }

        #endregion


        #region Helpers
        
        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

