using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow
{
    internal interface IEntityWorkflowTransitionProvider
    {
        Guid Guid { get; }
        string CodeName { get; }
        object ToStageId { get; }
        string CommandText { get; }
        bool HasConfirmation { get; }
        bool HasComment { get; }
        bool ShowInputForm { get; }
        Guid IconImageGuid { get; }
        Color? IconImageColor { get; }
        CommandPlacement CommandPlacement { get; }
        KeyGesture Gesture { get; }
        string Tooltip { get; }
        bool MultipleSelectionEnabled { get; }

        string GetConfimationText(IEnumerable<Entity> entities, object context);
        void GenerateInputForm(IInputForm form, IEnumerable<Entity> entities, object context);
        bool CanChangeStage(Entity entity, DataTransaction transaction, object context);
        bool CanChangeFrom(object stageId);
        DataTransaction Execute(EntityWorkflowTransition transition, 
            IEnumerable<Entity> entities, DataTransaction dataTransaction, IDictionary<string, object> inputValues, object context, bool force);
        bool ValidateEntities(IEnumerable<Entity> entities, bool throwOnError);
    }
}
