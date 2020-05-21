using System;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class StageProvider : IEntityWorkflowStageProvider
        {
            private ValueListItem _item;

            public object Id { get; set; }

            public string Name { get; set; }

            public string CodeName { get { return null; } }

            public Guid IconImageGuid
            {
                get { return _item?.IconImageGuid ?? Guid.Empty; }
            }

            public Color? IconImageColor
            {
                get { return _item?.Color; }
            }

            public ValueListItem Item
            {
                get { return _item; }
                set
                {
                    if (value != null)
                    {
                        if (!Equals(Id, value.WorkflowValue))
                            throw new ApplicationException();
                        Name = value.Name;
                    }
                    _item = value;
                }
            }

            public int CompareTo(IEntityWorkflowStageProvider other)
            {
                StageProvider provider = (StageProvider)other;
                if (_item == null || provider._item == null)
                    return ((IComparable)Id).CompareTo(provider.Id);
                return _item.Index.CompareTo(provider._item.Index);                    
            }

            public bool IsReadOnly(Entity entity)
            {
                return false;
            }
        }
    }
}
