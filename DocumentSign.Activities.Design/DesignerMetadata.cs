using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using DocumentSign.Activities.Design.Designers;
using DocumentSign.Activities.Design.Properties;

namespace DocumentSign.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            #region Setup

            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            #endregion Setup


            builder.AddCustomAttributes(typeof(SignScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(SignScope), new DesignerAttribute(typeof(SignScopeDesigner)));
            builder.AddCustomAttributes(typeof(SignScope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(CreateDocument), categoryAttribute);
            builder.AddCustomAttributes(typeof(CreateDocument), new DesignerAttribute(typeof(CreateDocumentDesigner)));
            builder.AddCustomAttributes(typeof(CreateDocument), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(CreateRecipient), categoryAttribute);
            builder.AddCustomAttributes(typeof(CreateRecipient), new DesignerAttribute(typeof(CreateRecipientDesigner)));
            builder.AddCustomAttributes(typeof(CreateRecipient), new HelpKeywordAttribute(""));

            //builder.AddCustomAttributes(typeof(DefineEnvelope), categoryAttribute);
            //builder.AddCustomAttributes(typeof(DefineEnvelope), new DesignerAttribute(typeof(DefineEnvelopeDesigner)));
            //builder.AddCustomAttributes(typeof(DefineEnvelope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(SendEnvelope), categoryAttribute);
            builder.AddCustomAttributes(typeof(SendEnvelope), new DesignerAttribute(typeof(SendEnvelopeDesigner)));
            builder.AddCustomAttributes(typeof(SendEnvelope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(DownloadEnvelopeDocuments), categoryAttribute);
            builder.AddCustomAttributes(typeof(DownloadEnvelopeDocuments), new DesignerAttribute(typeof(DownloadEnvelopeDocumentsDesigner)));
            builder.AddCustomAttributes(typeof(DownloadEnvelopeDocuments), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ListEnvelopes), categoryAttribute);
            builder.AddCustomAttributes(typeof(ListEnvelopes), new DesignerAttribute(typeof(ListEnvelopesDesigner)));
            builder.AddCustomAttributes(typeof(ListEnvelopes), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(GetEnvelopeInformation), categoryAttribute);
            builder.AddCustomAttributes(typeof(GetEnvelopeInformation), new DesignerAttribute(typeof(GetEnvelopeInformationDesigner)));
            builder.AddCustomAttributes(typeof(GetEnvelopeInformation), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
