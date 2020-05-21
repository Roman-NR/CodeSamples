using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class TransitionInfo : IEntityWorkflowTransition
        {
            private IEntityWorkflowStage[] _fromStages = new IEntityWorkflowStage[0];
            private IEntityWorkflowStage _toStage;
            private string _commandText;
            private TransitionConfirmationTextCallback _confirmationTextProvider;
            private Guid? _iconImageGuid;
            private Color? _iconImageColor;
            private CommandPlacement _commandPlacement;
            private bool _multipleSelectionEnabled;
            private IEntityWorkflowRole[] _roles;
            private InputFormGeneratorHandler _inputFormGenerate;
            private CanExecuteTransitionHandler _canExecute;
            private TransitionChangingHandler _onChanging;
            private TransitionChangingAcyncHandler _onChangingAsync;
            private TransitionChangedHandler _onChanged;
            private TransitionLogHandler _onLog;
            private List<TransitionChangingValidator> _validators;

            public TransitionInfo(string commandText)
            {
                _commandText = commandText;
                _commandPlacement = Presentation.CommandPlacement.Toolbar | Presentation.CommandPlacement.PropertiesToolbar | 
                                    Presentation.CommandPlacement.LinkList | Presentation.CommandPlacement.ContextMenu;
            }

            public EntityWorkflowTransition CreateTransition(EntityWorkflow workflow, WorkflowProvider workflowProvider,
                Dictionary<object, List<EntityWorkflowTransition>> transitionsTo,
                Dictionary<object, List<EntityWorkflowTransition>> transitionsFrom)
            {
                if (_toStage == null)
                    throw new ModelException(ErrorLevel.Warning, "Не указана стадия для перехода");

                TransitionProvider transitionProvider = new TransitionProvider
                {
                    Workflow = workflowProvider,
                    FromStageIds = new HashSet<object>(_fromStages.Select(s => s.StageId)),
                    ToStage = (StageProvider)workflow.GetStage(_toStage.StageId).Provider,
                    CommandText = _commandText,
                    IconImageGuid = _iconImageGuid.GetValueOrDefault(),
                    IconImageColor = _iconImageColor,
                    CommandPlacement = _commandPlacement,
                    MultipleSelectionEnabled = _multipleSelectionEnabled,
                    Roles = _roles ?? new IEntityWorkflowRole[0],
                    ConfirmationTextProvider = _confirmationTextProvider,
                    InputFormGenerate = _inputFormGenerate,
                    CanExecute = _canExecute,
                    Validators = _validators,
                    OnChanging = _onChanging,
                    OnChangingAsync = _onChangingAsync,
                    OnChanged = _onChanged,
                    OnLog = _onLog
                };

                EntityWorkflowTransition result = new EntityWorkflowTransition(workflow, transitionProvider);
                transitionsTo[_toStage.StageId].Add(result);
                foreach (IEntityWorkflowStage stage in _fromStages)
                    transitionsFrom[stage.StageId].Add(result);
                return result;
            }

            public IList<IEntityWorkflowStage> FromStages
            {
                get { return new ReadOnlyCollection<IEntityWorkflowStage>(_fromStages); }
            }

            public IEntityWorkflowStage ToStage
            {
                get { return _toStage; }
            }

            public IEntityWorkflowTransition From(params IEntityWorkflowStage[] stages)
            {
                _fromStages = stages ?? throw new ArgumentNullException(nameof(stages));
                return this;
            }

            public IEntityWorkflowTransition To(IEntityWorkflowStage stage)
            {
                _toStage = stage;
                return this;
            }

            public IEntityWorkflowTransition Confirm(string confirmationFormatString)
            {
                _confirmationTextProvider = new ConfirmationTextProvider() { FormatString = confirmationFormatString }.GetConfirmationText;
                return this;
            }

            public IEntityWorkflowTransition Confirm(TransitionConfirmationTextCallback confirmationTextProvider)
            {
                _confirmationTextProvider = confirmationTextProvider;
                return this;
            }

            public IEntityWorkflowTransition Icon(Guid? iconImageGuid, Color? color)
            {
                _iconImageGuid = iconImageGuid;
                _iconImageColor = color;
                return this;
            }

            public IEntityWorkflowTransition CommandPlacement(CommandPlacement commandPlacement)
            {
                _commandPlacement = commandPlacement;
                return this;
            }

            public IEntityWorkflowTransition MultipleSelectionEnabled()
            {
                _multipleSelectionEnabled = true;
                return this;
            }

            public IEntityWorkflowTransition Roles(params IEntityWorkflowRole[] roles)
            {
                _roles = roles;
                return this;
            }

            public IEntityWorkflowTransition InputForm(InputFormGeneratorHandler handler)
            {
                _inputFormGenerate = handler;
                return this;
            }

            public IEntityWorkflowTransition CanExecute(CanExecuteTransitionHandler handler)
            {
                _canExecute = handler;
                return this;
            }

            public IEntityWorkflowTransition OnChanging(TransitionChangingHandler handler)
            {
                _onChanging = handler;
                return this;
            }

            public IEntityWorkflowTransition OnChangingAsync(TransitionChangingAcyncHandler handler)
            {
                _onChangingAsync = handler;
                return this;
            }

            public IEntityWorkflowTransition OnChanged(TransitionChangedHandler handler)
            {
                _onChanged = handler;
                return this;
            }

            public IEntityWorkflowTransition OnLog(TransitionLogHandler handler)
            {
                _onLog = handler;
                return this;
            }

            public IEntityWorkflowTransition RequiredProperty(params Guid[] propertyGuids)
            {
                if (_validators == null)
                    _validators = new List<TransitionChangingValidator>();
                foreach (Guid propertyGuid in propertyGuids)
                    _validators.Add(new RequiredPropertyValidator(propertyGuid).Validate);
                return this;
            }

            public IEntityWorkflowTransition RequiredLink(params Guid[] linkGuids)
            {
                if (_validators == null)
                    _validators = new List<TransitionChangingValidator>();
                foreach (Guid linkGuid in linkGuids)
                    _validators.Add(new RequiredLinkValidator(linkGuid).Validate);
                return this;
            }

            public IEntityWorkflowTransition Validate(params TransitionChangingValidator[] validators)
            {
                if (_validators == null)
                    _validators = new List<TransitionChangingValidator>();
                foreach (TransitionChangingValidator validator in validators)
                    _validators.Add(validator);
                return this;
            }

            private class ConfirmationTextProvider
            {
                public string FormatString { get; set; }
                public string GetConfirmationText(IEnumerable<Entity> entities, object context)
                {
                    return string.Format(FormatString, string.Join(", ", entities));
                }
            }
        }
    }
}
