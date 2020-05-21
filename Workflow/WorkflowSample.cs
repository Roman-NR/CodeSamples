using System;
using System.Collections.Generic;
/* - */

namespace CodeSamples.Workflow
{
    public class TaskEntityWorkflowConfiguration : EntityWorkflowConfiguration
    {
        public IEntityWorkflowRole RoleResponsible { get; private set; }
        public IEntityWorkflowRole RoleAuthor { get; private set; }
        public IEntityWorkflowStage StagePlanned { get; private set; }
        public IEntityWorkflowStage StageForToday { get; private set; }
        public IEntityWorkflowStage StageInWork { get; private set; }
        public IEntityWorkflowStage StageCheck { get; private set; }
        public IEntityWorkflowStage StageDone { get; private set; }
        public IEntityWorkflowStage StageOverdue { get; private set; }
        public IEntityWorkflowStage StageCanceled { get; private set; }

        public TaskEntityWorkflowConfiguration(IServerConnection connection)
            : base(connection.NotNull(nameof(connection)).GetEntitySet(TaskSet.Guid))
        {
            Guid = TaskEntity.DefaultWorkflowGuid;

            StageProperty(TaskEntity.Properties.Status);

            RoleResponsible = Role((entity) => ((TaskEntity)entity).IsResponsible());
            RoleAuthor = Role((entity) => ((TaskEntity)entity).IsAuthor());

            StagePlanned = Stage(TaskEntityStatus.Planned);
            StageForToday = Stage(TaskEntityStatus.ForToday);
            StageInWork = Stage(TaskEntityStatus.InWork);
            StageCheck = Stage(TaskEntityStatus.Check);
            StageDone = Stage(TaskEntityStatus.Done);
            StageOverdue = Stage(TaskEntityStatus.Overdue);
            StageCanceled = Stage(TaskEntityStatus.Canceled);

            Initialize();
        }

        protected virtual void Initialize()
        {
            Transition("В работу")
                .From(StagePlanned, StageForToday).To(StageInWork)
                .OnChanging(OnTakingInWork)
                .Confirm("Взять задачу {0} в работу?")
                .Roles(RoleResponsible);

            Transition("Закрыть")
                .From(StagePlanned, StageForToday, StageInWork, StageOverdue).To(StageDone)
                .Confirm("Закрыть задачу {0}?")
                .InputForm(GenerateDoneForm)
                .CanExecute(CanChangeToDone)
                .OnChanging(OnChangingToDone)
                .Roles(RoleResponsible, RoleAuthor);

            Transition("На проверку")
                .From(StagePlanned, StageForToday, StageInWork, StageOverdue).To(StageCheck)
                .Confirm("Отправить на проверку задачу {0}?")
                .InputForm(GenerateDoneForm)
                .CanExecute(CanChangeToCheck)
                .OnChanging(OnChangingToDone)
                .Roles(RoleResponsible);

            Transition("Вернуть в работу")
                .From(StageCheck).To(StageInWork)
                .Confirm("Вернуть задачу {0} в работу?")
                .InputForm(GenerateDoneForm)
                .OnChanging(OnReturningInWork)
                .Roles(RoleAuthor);

            Transition("Закрыть")
                .From(StageCheck).To(StageDone)
                .Confirm("Закрыть задачу {0}?")
                .InputForm(GenerateDoneForm)
                .OnChanging(OnChangingToDone)
                .Roles(RoleAuthor);

            Transition("Отменить")
                .From(StagePlanned, StageForToday, StageInWork, StageOverdue).To(StageCanceled)
                .Confirm("Отменить задачу {0}?")
                .InputForm(GenerateCancelForm)
                .OnChanging(OnChangingToCanceled)
                .Roles(RoleResponsible, RoleAuthor);

            Transition("Перепланировать")
                .From(StageCanceled, StageDone).To(StagePlanned)
                .Confirm("Переназначить задачу {0}?")
                .OnChanging(OnChangingToPlanned)
                .Roles(RoleAuthor);
        }

        protected static bool OnTakingInWork(Entity entity, IDictionary<string, object> inputValues, object context)
        {
            TaskEntity task = (TaskEntity)entity;
            if (task.GetResponsible()?.Guid != task.Connection.Workspace.UserEntityGuid)
                task.SetResponsible(task.Connection.Workspace.GetUser());
            return true;
        }

        protected static void GenerateCancelForm(IInputForm form, IEnumerable<Entity> entities, object context)
        {
            IInputFormControl<string> commentControl = form.AddStringControl("Причина", true);
            commentControl.Name = "Comment";
            commentControl.IsRequired = true;
        }

        protected static bool OnChangingToCanceled(Entity entity, IDictionary<string, object> inputValues, object context)
        {
            TaskEntity task = (TaskEntity)entity;

            string comment = (string)inputValues["Comment"];
            if (string.IsNullOrWhiteSpace(comment))
                return false;

            task.AddComment(comment, TaskEntityStatus.Canceled);
            task.EndTime = DateTime.Now;

            return true;
        }

        protected static void GenerateDoneForm(IInputForm form, IEnumerable<Entity> entities, object context)
        {
            IInputFormControl<string> commentControl = form.AddStringControl("Комментарий", true);
            commentControl.Name = "Comment";
        }

        protected static bool CanChangeToDone(Entity entity, object context)
        {
            TaskEntity task = (TaskEntity)entity;
            return !task.Type.Properties.Contains(TaskEntity.Properties.IsCheckRequired) || !task.IsCheckRequired || task.IsAuthor();
        }

        private bool CanChangeToCheck(Entity entity, object context)
        {
            TaskEntity task = (TaskEntity)entity;
            return task.Type.Properties.Contains(TaskEntity.Properties.IsCheckRequired) && task.IsCheckRequired;
        }

        protected static bool OnChangingToDone(Entity entity, IDictionary<string, object> inputValues, object context)
        {
            TaskEntity task = (TaskEntity)entity;

            string comment = (string)inputValues["Comment"];
            if (!string.IsNullOrWhiteSpace(comment))
                task.AddComment(comment, TaskEntityStatus.Done);

            task.EndTime = DateTime.Now;

            return true;
        }

        protected static bool OnReturningInWork(Entity entity, IDictionary<string, object> inputValues, object context)
        {
            TaskEntity task = (TaskEntity)entity;

            string comment = (string)inputValues["Comment"];
            if (!string.IsNullOrWhiteSpace(comment))
                task.AddComment(comment, TaskEntityStatus.InWork);

            task.EndTime = DateTime.MinValue;

            return true;
        }

        protected static bool OnChangingToPlanned(Entity entity, IDictionary<string, object> inputValues, object context)
        {
            TaskEntity task = (TaskEntity)entity;

            task.EndTime = DateTime.MinValue;

            return true;
        }
    }
}
