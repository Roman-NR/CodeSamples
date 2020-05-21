using System;
using System.Collections.Generic;
using System.Linq;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private EntitySetInfo _entitySet;
        private Guid _guid;
        private string _name;
        private string _propertyName;
        private Guid _stagePropertyGuid;
        private Guid _stageListGuid;
        private List<IEntityWorkflowTypeRule> _types = new List<IEntityWorkflowTypeRule>();
        private List<IEntityWorkflowRole> _roles = new List<IEntityWorkflowRole>();
        private Dictionary<object, IEntityWorkflowStage> _stages = new Dictionary<object, IEntityWorkflowStage>();
        private List<IEntityWorkflowTransition> _transitions = new List<IEntityWorkflowTransition>();
        private IStageChangeHistoryConfiguration _historyCofiguration;
        private List<Guid> _showInLinks = new List<Guid>();

        public EntityWorkflowConfiguration(EntitySetInfo entitySet)
        {
            _entitySet = entitySet ?? throw new ArgumentNullException(nameof(entitySet));
        }

        public EntitySetInfo EntitySet
        {
            get { return _entitySet; }
        }

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public void StageProperty(Guid propertyGuid)
        {
            StageProperty(propertyGuid, Guid.Empty);
        }

        public void StageProperty(Guid propertyGuid, Guid valueListGuid)
        {
            _stagePropertyGuid = propertyGuid;
            _stageListGuid = valueListGuid;
        }

        public IEntityWorkflowTypeRule ForType(Guid typeGuid)
        {
            IEntityWorkflowTypeRule result = new TypeRule(typeGuid);
            _types.Add(result);
            return result;
        }

        public IEntityWorkflowRole Role(Guid systemRoleGuid)
        {
            IEntityWorkflowRole result = new SystemRole(systemRoleGuid);
            _roles.Add(result);
            return result;
        }

        public IEntityWorkflowRole Role(Predicate<Entity> match)
        {
            IEntityWorkflowRole result = new WorkflowRole(match ?? throw new ArgumentNullException(nameof(match)));
            _roles.Add(result);
            return result;
        }

        public IEntityWorkflowStage Stage(Enum propertyValue)
        {
            return Stage(Convert.ToInt32(propertyValue));
        }

        public IEntityWorkflowStage Stage(object propertyValue)
        {
            if (_stages.ContainsKey(propertyValue))
                throw new InvalidOperationException();

            IEntityWorkflowStage result = new StageInfo(propertyValue);
            _stages.Add(propertyValue, result);
            return result;
        }

        public IEntityWorkflowTransition Transition(string commandName)
        {
            IEntityWorkflowTransition result = new TransitionInfo(commandName);
            _transitions.Add(result);
            return result;
        }

        public ICollection<IEntityWorkflowStage> Stages
        {
            get { return _stages.Values; }
        }

        public IList<IEntityWorkflowTransition> Transitions
        {
            get { return _transitions.AsReadOnly(); }
        }

        public IStageChangeHistoryConfiguration StageChangeHistory(
            Guid entitySetGuid,
            Guid stagePropertyGuid,
            Guid ownerLinkGuid,
            Guid responsibleLinkGuid = default(Guid),
            Guid dateTimePropertyGuid = default(Guid),
            Guid commentPropertyGuid = default(Guid),
            Guid entityTypeGuid = default(Guid))
        {
            return _historyCofiguration = new StageChangeHistoryConfiguration()
            {
                EntitySetGuid = entitySetGuid,
                EntityTypeGuid = entityTypeGuid,
                StagePropertyGuid = stagePropertyGuid,
                CommentPropertyGuid = commentPropertyGuid,
                DateTimePropertyGuid = dateTimePropertyGuid,
                ResponsibleLinkGuid = responsibleLinkGuid,
                OwnerLinkGuid = ownerLinkGuid
            };            
        }

        public void ShowInLink(Guid linkGuid)
        {
            _showInLinks.Add(linkGuid);
        }

        public EntityWorkflow CreateWorkflow()
        {
            WorkflowProvider workflowProvider = new WorkflowProvider
            {
                Guid = _guid,
                Name = _name,
                PropertyName = _propertyName,
                EntitySet = _entitySet,
                StageListGuid = _stageListGuid,
                StagePropertyGuid = _stagePropertyGuid,
                Types = _types.ToArray(),
                ShowInLinks = new HashSet<Guid>(_showInLinks),
                HistoryConfiguration = _historyCofiguration
            };

            EntityWorkflow result = new EntityWorkflow(workflowProvider);

            ValueList valueList;            
            if (_stageListGuid == Guid.Empty)
            {
                EntitySetScheme scheme = _entitySet.GetScheme();
                if (_types.Count == 0)
                    valueList = (scheme.Properties.Find(_stagePropertyGuid)?.Format as ValueListPropertyFormat)?.ValueList?.GetList();
                else
                {
                    EntityType type = _types.SelectMany(t => t.GetTypes(scheme.Types)).FirstOrDefault();
                    valueList = (type?.Properties.Find(_stagePropertyGuid)?.Format as ValueListPropertyFormat)?.ValueList?.GetList();
                }
            }
            else
            {
                valueList = ValueList.GetValueList(_entitySet.Connection, _stageListGuid);
                if (valueList == null)
                    throw new ModelException(ErrorLevel.Warning, "Не найден список значений {0}", _stageListGuid);
            }

            ILookup<object, ValueListItem> items = valueList?.ToLookup(i => i.WorkflowValue);

            List<EntityWorkflowStage> stages = new List<EntityWorkflowStage>();
            foreach (KeyValuePair<object, IEntityWorkflowStage> source in _stages)
            {
                StageProvider stageProvider = new StageProvider
                {
                    Id = source.Key,
                    Item = items?[source.Key].FirstOrDefault()
                };
                stages.Add(new EntityWorkflowStage(result, stageProvider));
            }
            result.Stages = stages;

            Dictionary<object, List<EntityWorkflowTransition>> transitionsTo = stages.ToDictionary(s => s.Id, s => new List<EntityWorkflowTransition>());
            Dictionary<object, List<EntityWorkflowTransition>> transitionsFrom = stages.ToDictionary(s => s.Id, s => new List<EntityWorkflowTransition>());

            result.Transitions = _transitions.OfType<TransitionInfo>().Select(
                t => t.CreateTransition(result, workflowProvider, transitionsTo, transitionsFrom)).ToList();

            foreach (EntityWorkflowStage stage in stages)
            {
                stage.SetTransitionsTo(transitionsTo[stage.Id]);
                stage.SetTransitionsFrom(transitionsFrom[stage.Id]);
            }

            return result;
        }
    }
}
