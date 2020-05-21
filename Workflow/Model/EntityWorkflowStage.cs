using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow
{
    public class EntityWorkflowStage : IComparable<EntityWorkflowStage>, IComparable
    {
        private EntityWorkflow _workflow;
        private IEntityWorkflowStageProvider _provider;
        private EntityWorkflowTransition[] _transitionsFrom;
        private EntityWorkflowTransition[] _transitionsTo;

        internal EntityWorkflowStage(EntityWorkflow workflow, IEntityWorkflowStageProvider provider)
        {
            _workflow = workflow;
            _provider = provider;
        }

        internal IEntityWorkflowStageProvider Provider
        {
            get { return _provider; }
        }

        public EntityWorkflow Workflow
        {
            get { return _workflow; }
        }

        public object Id
        {
            get { return _provider.Id; }
        }

        public Guid IconImageGuid
        {
            get { return _provider.IconImageGuid; }
        }

        public Color? IconImageColor
        {
            get { return _provider.IconImageColor; }
        }

        public string Name
        {
            get { return _provider.Name; }
        }

        public string CodeName
        {
            get { return _provider.CodeName; }
        }

        public IList<EntityWorkflowTransition> TransitionsFrom
        {
            get { return new ReadOnlyCollection<EntityWorkflowTransition>(_transitionsFrom); }
        }

        internal void SetTransitionsFrom(IEnumerable<EntityWorkflowTransition> transitions)
        {
            _transitionsFrom = transitions.ToArray();
        }

        public IList<EntityWorkflowTransition> TransitionsTo
        {
            get { return new ReadOnlyCollection<EntityWorkflowTransition>(_transitionsTo); }
        }

        internal void SetTransitionsTo(IEnumerable<EntityWorkflowTransition> transitions)
        {
            _transitionsTo = transitions.ToArray();
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator >(EntityWorkflowStage first, EntityWorkflowStage second)
        {
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) > 0;
        }

        public static bool operator >=(EntityWorkflowStage first, EntityWorkflowStage second)
        {
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) >= 0;
        }

        public static bool operator <(EntityWorkflowStage first, EntityWorkflowStage second)
        {
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) < 0;
        }

        public static bool operator <=(EntityWorkflowStage first, EntityWorkflowStage second)
        {
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) <= 0;
        }

        public static bool operator >(EntityWorkflowStage first, object secondId)
        {
            EntityWorkflowStage second = first?.Workflow.GetStage(secondId);
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) > 0;
        }

        public static bool operator >=(EntityWorkflowStage first, object secondId)
        {
            EntityWorkflowStage second = first?.Workflow.GetStage(secondId);
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) >= 0;
        }

        public static bool operator <(EntityWorkflowStage first, object secondId)
        {
            EntityWorkflowStage second = first?.Workflow.GetStage(secondId);
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) < 0;
        }

        public static bool operator <=(EntityWorkflowStage first, object secondId)
        {
            EntityWorkflowStage second = first?.Workflow.GetStage(secondId);
            return Comparer<EntityWorkflowStage>.Default.Compare(first, second) <= 0;
        }

        int IComparable<EntityWorkflowStage>.CompareTo(EntityWorkflowStage other)
        {
            return CompareTo(other);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as EntityWorkflowStage);
        }

        public int CompareTo(EntityWorkflowStage other)
        {
            if (other == null)
                return -1;
            int result = _workflow.Guid.CompareTo(other._workflow.Guid);
            if (result == 0)
                result = _provider.CompareTo(other._provider);
            return result;
        }
    }
}
