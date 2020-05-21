using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
/* -- */

namespace CodeSamples.Workflow
{
    public class EntityWorkflow
    {
        private IEntityWorkflowProvider _provider;
        private List<EntityWorkflowStage> _stages;
        private Dictionary<object, EntityWorkflowStage> _stageMap;
        private EntityWorkflowTransition[] _transitions;
        private IdProvider _idProvider;

        internal EntityWorkflow(IEntityWorkflowProvider provider)
        {
            _provider = provider;
        }

        public EntitySetInfo EntitySet
        {
            get { return _provider.EntitySet; }
        }

        public Guid Guid
        {
            get { return _provider.Guid; }
        }

        public string Name
        {
            get { return _provider.Name; }
        }

        public string CodeName
        {
            get { return _provider.CodeName; }
        }

        public string PropertyName
        {
            get { return _provider.PropertyName; }
        }

        public string PropertyCodeName
        {
            get { return _provider.PropertyCodeName; }
        }

        public IList<EntityWorkflowStage> Stages
        {
            get { return _stages.AsReadOnly(); }
            internal set
            {
                _stages = value.ToList();
                _stages.Sort();
                _stageMap = value.ToDictionary(s => s.Id);
            }
        }

        public IList<EntityWorkflowTransition> Transitions
        {
            get { return new ReadOnlyCollection<EntityWorkflowTransition>(_transitions); }
            internal set { _transitions = value.ToArray(); }
        }

        public EntityWorkflowStage GetEntityStage(Entity entity)
        {
            return GetEntityStage(entity, null);
        }

        public EntityWorkflowStage GetEntityStage(Entity entity, DataTransaction transaction)
        {
            object stageId;
            return GetEntityStageId(entity, transaction, out stageId) ? GetStage(stageId) : null;
        }

        public bool GetEntityStageId(Entity entity, out object stageId)
        {
            return GetEntityStageId(entity, null, out stageId);
        }

        public bool GetEntityStageId(Entity entity, DataTransaction transaction, out object stageId)
        {
            return _provider.GetEntityStageId(entity, transaction, out stageId);
        }

        public bool IsCommandVisibleInLinks
        {
            get { return _provider.IsCommandVisibleInLinks; }
        }

        public bool IsCommandVisible(EntitySetLink link)
        {
            return link != null && IsCommandVisible(link.Guid);
        }

        public bool IsCommandVisible(Guid linkGuid)
        {
            return _provider.IsCommandVisible(linkGuid);
        }

        public EntityWorkflowStage GetStage(object stageId)
        {
            if (stageId is Enum)
                stageId = Convert.ToInt32(stageId);
            return _stageMap.TryGetValue(stageId, out EntityWorkflowStage result) ? result : null;
        }

        public DataTransaction SetStage(Entity entity, object stageId, DataTransaction transaction = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return SetStage(new[] { entity }, stageId, transaction);
        }

        public DataTransaction SetStage(IEnumerable<Entity> entities, object stageId, DataTransaction transaction = null)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (stageId == null)
                throw new ArgumentNullException(nameof(stageId));

            EntityWorkflowStage stage = GetStage(stageId);
            if (stage == null)
                throw new ArgumentException("Unknown stage " + stageId, nameof(stageId));

            Entity entity = entities.FirstOrDefault();
            if (entity == null)
                return null;
            if (transaction == null)
                transaction = entity.Transaction;

            return _provider.ForceChangeStage(entities, stage, transaction);
        }

        internal IdProvider Ids
        {
            get { return _idProvider ?? (_idProvider = IdProvider.GetWorkflowIdProvider(this)); }
        }

        internal ConditionItemData GetConditionItem(List<Guid> links, SearchCondition condition)
        {
            return _provider.GetConditionItem(links, condition);
        }

        internal IEnumerable<EntityWorkflowStageChange> GetStageChanges(Entity entity, IEnumerable<WorkflowStageChangeData> data)
        {
            return _provider.GetStageChanges(this, entity, data);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
