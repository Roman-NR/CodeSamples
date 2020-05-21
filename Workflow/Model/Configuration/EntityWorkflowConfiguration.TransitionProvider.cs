using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class TransitionProvider : IEntityWorkflowTransitionProvider
        {
            private Guid _iconImageGuid;
            private Color? _iconImageColor;

            public Guid Guid { get; set; }
            public string CodeName { get { return null; } }
            public WorkflowProvider Workflow { get; set; }
            public StageProvider ToStage { get; set; }
            public HashSet<object> FromStageIds { get; set; }
            public string CommandText { get; set; }
            public TransitionConfirmationTextCallback ConfirmationTextProvider { get; set; }
            public CommandPlacement CommandPlacement { get; set; }
            public KeyGesture Gesture { get; set; }
            public string Tooltip { get; set; }
            public bool MultipleSelectionEnabled { get; set; }
            public IEntityWorkflowRole[] Roles { get; set; }
            public CanExecuteTransitionHandler CanExecute { get; set; }
            public InputFormGeneratorHandler InputFormGenerate { get; set; }
            public IList<TransitionChangingValidator> Validators { get; set; }
            public TransitionChangingHandler OnChanging { get; set; }
            public TransitionChangingAcyncHandler OnChangingAsync { get; set; }
            public TransitionChangedHandler OnChanged { get; set; }
            public TransitionLogHandler OnLog { get; set; }

            public object ToStageId
            {
                get { return ToStage.Id; }
            }

            public bool HasConfirmation
            {
                get { return ConfirmationTextProvider != null; }
            }

            public bool HasComment
            {
                get
                {
                    return InputFormGenerate == null &&
                           Workflow.HistoryConfiguration != null &&
                           Workflow.HistoryConfiguration.CommentPropertyGuid != System.Guid.Empty;
                }
            }

            public bool ShowInputForm
            {
                get { return InputFormGenerate != null || HasComment; }
            }

            public string GetConfimationText(IEnumerable<Entity> entities, object context)
            {
                return ConfirmationTextProvider?.Invoke(entities, context) ?? string.Empty;
            }

            public Guid IconImageGuid
            {
                get
                {
                    if (_iconImageGuid != System.Guid.Empty)
                        return _iconImageGuid;

                    ValueListItem item = ToStage.Item;
                    if (item == null || !item.List.HasImages)
                        return System.Guid.Empty;

                    if (item.IconImageGuid == System.Guid.Empty)
                        return new Guid("f639a724-3d27-4a0c-af05-a25dbb2bda06"); // RectangleDrawingImage"

                    return item.IconImageGuid;
                }
                internal set { _iconImageGuid = value; }
            }

            public Color? IconImageColor
            {
                get { return _iconImageColor ?? ToStage.Item?.Color; }
                internal set { _iconImageColor = value; }
            }

            public void GenerateInputForm(IInputForm form, IEnumerable<Entity> entities, object context)
            {
                InputFormGenerate?.Invoke(form, entities, context);
            }

            public bool CanChangeStage(Entity entity, DataTransaction transaction, object context)
            {
                if (!Workflow.GetEntityStageId(entity, transaction, out object stageId) || !CanChangeFrom(stageId))
                    return false;

                if (Roles.Length > 0 && !Roles.Any(role => role.Check(entity)))
                    return false;

                return CanExecute == null || CanExecute(entity, context);
            }

            public bool CanChangeFrom(object stageId)
            {
                return FromStageIds.Count == 0 || FromStageIds.Contains(stageId);
            }

            public DataTransaction Execute(EntityWorkflowTransition transition, 
                IEnumerable<Entity> entities, DataTransaction dataTransaction, 
                IDictionary<string, object> inputValues, object context, bool force)
            {
                if (inputValues == null)
                    inputValues = new Dictionary<string, object>(0);

                using (DataTransactionSwitch transaction = new DataTransactionSwitch(Workflow.EntitySet.Connection, dataTransaction))
                {
                    foreach (Entity entity in entities)
                    {
                        if (!force &&!CanChangeStage(entity, transaction, context))
                            throw new ModelException(ErrorLevel.Information, "Нельзя сменить стадию объекта на {0}", ToStage.Name);

                        entity.Edit(transaction);
                        ValidateEntities(new[] { entity }, true);

                        if (OnChanging != null && !OnChanging(entity, inputValues, context))
                            return null;
                        if (OnChangingAsync != null)
                        {
                            bool result = false;
                            if (System.Windows.Application.Current != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
                            {
                                using (ManualResetEventSlim complete = new ManualResetEventSlim(false))
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(async () =>
                                    {
                                        try
                                        {
                                            result = await OnChangingAsync(entity, inputValues, context);
                                        }
                                        finally
                                        {
                                            complete.Set();
                                        }
                                    }));
                                    complete.Wait();
                                }
                            }
                            else
                                throw new ModelException(ErrorLevel.Warning, "Попытка вызвать смену стадии по переходу с обработчиком OnChangingAsync из основного потока");

                            if (!result)
                                return null;
                        }
                        if (!entity.OnWorkflowStageChanging(transition.ToStage, transition, inputValues))
                            return null;
                        if (entity.EntitySet.Extension != null && !entity.EntitySet.Extension.OnWorkflowStageChanging(entity, transition.ToStage, transition, inputValues, context))
                            return null;

                        Workflow.SetEntityStageId(entity, ToStageId, CommandText, inputValues, OnLog);
                    }

                    transaction.Commit();

                    if (OnChanged != null)
                    {
                        if (System.Windows.Application.Current != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
                        {
                            using (ManualResetEventSlim complete = new ManualResetEventSlim(false))
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        OnChanged(entities, inputValues, context);
                                    }
                                    finally
                                    {
                                        complete.Set();
                                    }
                                });
                                complete.Wait();
                            }
                        }
                        else
                            OnChanged(entities, inputValues, context);
                    }

                    return transaction;
                }
            }

            public bool ValidateEntities(IEnumerable<Entity> entities, bool throwOnError)
            {
                if (Validators != null)
                {
                    List<string> errors = new List<string>();
                    foreach (Entity entity in entities)
                    {
                        foreach (TransitionChangingValidator validator in Validators)
                        {
                            if (!validator(entity, out string errorText))
                                errors.Add(errorText);
                        }
                    }
                    if (errors.Count > 0)
                    {
                        if (throwOnError)
                            throw new ModelException(ErrorLevel.Warning, string.Join(Environment.NewLine, errors));
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
