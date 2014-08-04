using System;
using System.Web.Mvc;
using DcmsMobile.Repack.ViewModels;

namespace DcmsMobile.Repack.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class UiHintForUiAttribute : Attribute, IMetadataAware
    {
        private readonly RepackUiStyle _allowableStyles;
        private readonly string _defaultTemplateName;
        private readonly string _templateName;

        /// <summary>
        /// Applies the template based on the specified UI
        /// </summary>
        /// <param name="uiStyles">The styles for which template must be applied</param>
        /// <param name="templateName">The template to apply for the specified UI styles</param>
        /// <remarks>
        /// If the Ui is not one of the specified UI, all display metadata properties are set to false.
        /// </remarks>
        public UiHintForUiAttribute(RepackUiStyle uiStyles, string templateName)
        {
            _allowableStyles = uiStyles;
            _templateName = templateName;
        }

        /// <summary>
        /// Conditionally set the Template name
        /// </summary>
        /// <param name="uiStyles"></param>
        /// <param name="templateName"></param>
        /// <param name="defaultTemplateName"></param>
        public UiHintForUiAttribute(RepackUiStyle uiStyles, string templateName, string defaultTemplateName)
        {
            _allowableStyles = uiStyles;
            _templateName = templateName;
            _defaultTemplateName = defaultTemplateName;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.TemplateHint = _templateName;
            if (!IsVisible)
            {
                // None of the styles matched. Make it invisible
                metadata.ShowForDisplay = false;
                metadata.ShowForEdit = false;
                metadata.HideSurroundingHtml = true;
                if (!string.IsNullOrEmpty(_defaultTemplateName))
                {
                    // Use the default template
                    metadata.TemplateHint = _defaultTemplateName;
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                var currentStyles = RepackViewModel.CurrentUiStyle;
                var commonStyles = currentStyles & _allowableStyles;
                return commonStyles != 0;
            }
        }
    }

}