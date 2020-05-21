using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public delegate string TransitionConfirmationTextCallback(IEnumerable<Entity> entities, object context);
    public delegate bool CanExecuteTransitionHandler(Entity entity, object context);
    public delegate bool TransitionChangingValidator(Entity entity, out string errorText);
    public delegate bool TransitionChangingHandler(Entity entity, IDictionary<string, object> inputValues, object context);
    public delegate Task<bool> TransitionChangingAcyncHandler(Entity entity, IDictionary<string, object> inputValues, object context);
    public delegate void TransitionChangedHandler(IEnumerable<Entity> entities, IDictionary<string, object> inputValues, object context);
    public delegate void InputFormGeneratorHandler(IInputForm form, IEnumerable<Entity> entities, object context);
    public delegate void TransitionLogHandler(Entity entity, Entity logEntity, IDictionary<string, object> inputValues);    

    public interface IEntityWorkflowTransition
    {
        IList<IEntityWorkflowStage> FromStages { get; }
        IEntityWorkflowStage ToStage { get; }

        IEntityWorkflowTransition From(params IEntityWorkflowStage[] stages);
        IEntityWorkflowTransition To(IEntityWorkflowStage stage);
        IEntityWorkflowTransition Confirm(string confirmationFormatString);
        IEntityWorkflowTransition Confirm(TransitionConfirmationTextCallback confirmationTextProvider);
        IEntityWorkflowTransition Icon(Guid? iconImageGuid, Color? color);
        IEntityWorkflowTransition CommandPlacement(CommandPlacement commandPlacement);
        IEntityWorkflowTransition MultipleSelectionEnabled();
        IEntityWorkflowTransition Roles(params IEntityWorkflowRole[] roles);
        IEntityWorkflowTransition CanExecute(CanExecuteTransitionHandler match);
        IEntityWorkflowTransition InputForm(InputFormGeneratorHandler handler);
        IEntityWorkflowTransition OnChanging(TransitionChangingHandler match);
        IEntityWorkflowTransition OnChangingAsync(TransitionChangingAcyncHandler match);
        IEntityWorkflowTransition OnChanged(TransitionChangedHandler handler);
        IEntityWorkflowTransition OnLog(TransitionLogHandler handler);
        IEntityWorkflowTransition RequiredProperty(params Guid[] propertyGuids);
        IEntityWorkflowTransition RequiredLink(params Guid[] linkGuids);
        IEntityWorkflowTransition Validate(params TransitionChangingValidator[] validators);
    }
}