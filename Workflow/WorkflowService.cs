using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
/* - */

namespace CodeSamples.Workflow
{
    public static class WorkflowService
    {
        public static bool CanExecute(EntityWorkflowTransition transition, IEnumerable<Entity> entities, IEntitySetExtensionContext context)
        {
            int count = 0;
            foreach (Entity entity in entities)
            {
                if (entity == null || 
                    entity.EntitySet.Scheme.Guid != transition.Workflow.EntitySet.Guid ||
                    !transition.CanChangeStage(entity, context))
                {
                    return false;
                }
                if (entity.Transaction != null &&
                    (context?.PropertiesView == null || context.PropertiesView.InheritTransaction))
                {
                    return false;
                }

                ++count;
                if (count > 1 && !transition.MultipleSelectionEnabled)
                    return false;
            }
            return count > 0;
        }

        public static async Task<DataTransaction> ChangeStageAsync(EntityWorkflowTransition transition, IEnumerable<Entity> entities, IEntitySetExtensionContext context)
        {
            if (!entities.Any())
                return null;

            Dictionary<string, object> inputValues;
            if (transition.ShowInputForm)
            {
                if (!await Task.Run(() => transition.ValidateEntities(entities)))
                    return null;

                IInputForm form = InputFormFactory.Instance.CreateForm();
                form.Caption = transition.CommandText;

                if (transition.HasConfirmation)
                    form.AddTextBlock(transition.GetConfimationText(entities, context));

                transition.GenerateInputForm(form, entities, context);

                if (transition.HasComment && !form.Controls.Any(c => c.Name == "Comment"))
                    form.AddStringControl("Комментарий", true).Name = "Comment";

                if (form.Controls.Any(c => c is IInputFormSelectEntityControl))
                {
                    if (!await form.Show())
                        return null;
                }
                else
                {
                    if (!form.ShowDialog())
                        return null;
                }

                inputValues = form.Controls.ToDictionary(c => string.IsNullOrEmpty(c.Name) ? c.Label : c.Name, c => c.Value);
            }
            else
            {
                inputValues = new Dictionary<string, object>(0);
                if (transition.HasConfirmation)
                {
                    if (MessageBox.Show(Application.Current.MainWindow, transition.GetConfimationText(entities, context), transition.CommandText,
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return null;
                    }
                }
            }

            return await ProgressFeedback.Run("Смена стадии", progress =>
            {
                progress.Cancelable = false;

                string transitionName = transition.ToString();
                string toStageName = transition.ToStage?.ToString();

                if (string.IsNullOrWhiteSpace(toStageName) || transitionName == toStageName)
                    progress.Progress(transitionName);
                else
                    progress.Progress($"{transitionName} — {toStageName}");

                return transition.Execute(entities, null, inputValues, context);
            });
        }
    }
}
