using System.Collections.Generic;
using System.Web.Mvc;
using Models.Attributes;

namespace DLMS.Validators
{
    public class FFRequiredValidator :
  DataAnnotationsModelValidator<FFRequiredAttribute>
    {
        private string errorMessage;

        public FFRequiredValidator(
            ModelMetadata metadata, ControllerContext context, FFRequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
            string fieldName = metadata.DisplayName == null ? metadata.PropertyName : metadata.DisplayName;
            this.errorMessage = attribute.FormatErrorMessage(fieldName);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.errorMessage,
                ValidationType = "required"
            };
            yield return rule;
        }
    }
}