using System;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        public class RequiredPropertyValidator
        {
            private Guid _propertyGuid;

            public RequiredPropertyValidator(Guid propertyGuid)
            {
                _propertyGuid = propertyGuid;
            }

            public bool Validate(Entity entity, out string errorText)
            {
                EntityTypeProperty property = entity.Type.Properties.Find(_propertyGuid);
                if (property != null && property.Format.IsEmpty(entity[property.Property]))
                {
                    errorText = "Не заполнено свойство " + property.Property.Name;
                    return false;
                }
                errorText = null;
                return true;
            }
        }

        private class RequiredLinkValidator
        {
            private Guid _linkGuid;

            public RequiredLinkValidator(Guid linkGuid)
            {
                _linkGuid = linkGuid;
            }

            public bool Validate(Entity entity, out string errorText)
            {
                EntityTypeLink link = entity.Type.Links.Find(_linkGuid);
                if (link != null && link.Link.Type.IsToOne() ?
                        entity.GetLink(link.Link.Guid) == null :
                        entity.GetLinks(link.Link.Guid).Count == 0)
                {
                    errorText = "Не заполнена связь " + link.Link.Name;
                    return false;
                }
                errorText = null;
                return true;
            }
        }
    }
}
