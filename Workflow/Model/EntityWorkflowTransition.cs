using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow
{
    public class EntityWorkflowTransition
    {
        private EntityWorkflow _workflow;
        private IEntityWorkflowTransitionProvider _provider;

        internal EntityWorkflowTransition(EntityWorkflow workflow, IEntityWorkflowTransitionProvider provider)
        {
            _workflow = workflow;
            _provider = provider;
        }

        public Guid Guid
        {
            get { return _provider.Guid; }
        }

        public string CodeName
        {
            get { return _provider.CodeName; }
        }

        public EntityWorkflow Workflow
        {
            get { return _workflow; }
        }

        internal IEntityWorkflowTransitionProvider Provider
        {
            get { return _provider; }
        }

        public EntityWorkflowStage ToStage
        {
            get { return _workflow.GetStage(_provider.ToStageId); }
        }

        public string CommandText
        {
            get { return _provider.CommandText; }
        }

        public bool HasConfirmation
        {
            get { return _provider.HasConfirmation; }
        }

        public bool HasComment
        {
            get { return _provider.HasComment; }
        }

        public bool ShowInputForm
        {
            get { return _provider.ShowInputForm; }
        }

        public string GetConfimationText(IEnumerable<Entity> entities, object context)
        {
            return _provider.GetConfimationText(entities, context);
        }
        
        public Guid IconImageGuid
        {
            get { return _provider.IconImageGuid; }
        }

        public Color? IconImageColor
        {
            get { return _provider.IconImageColor; }
        }

        public CommandPlacement CommandPlacement
        {
            get { return _provider.CommandPlacement; }
        }

        public KeyGesture Gesture 
        {
            get { return _provider.Gesture; }
        }

        public string Tooltip 
        {
            get { return _provider.Tooltip; } 
        }

        public bool MultipleSelectionEnabled
        {
            get { return _provider.MultipleSelectionEnabled; }
        }

        public void GenerateInputForm(IInputForm form, IEnumerable<Entity> entities, object context)
        {
            _provider.GenerateInputForm(form, entities, context);
        }

        public bool CanChangeStage(Entity entity, object context)
        {
            return CanChangeStage(entity, null, context);
        }

        public bool CanChangeStage(Entity entity, DataTransaction transaction, object context)
        {
            return _provider.CanChangeStage(entity, transaction, context);
        }

        public bool CanChangeFrom(object stageId)
        {
            return _provider.CanChangeFrom(stageId);
        }

        public DataTransaction Execute(
            Entity entity, DataTransaction transaction = null,
            IDictionary<string, object> inputValues = null, object context = null,
            bool force = false)
        {
            return Execute(new [] { entity }, transaction, inputValues, context, force);
        }

        public DataTransaction Execute(
            IEnumerable<Entity> entities, DataTransaction transaction = null, 
            IDictionary<string, object> inputValues = null, object context = null, 
            bool force = false)
        {
            Entity entity = entities.FirstOrDefault();
            if (entity == null)
                return null;
            if (transaction == null)
                transaction = entity.Transaction;

            return _provider.Execute(this, entities, transaction, inputValues, context, force);
        }

        public Task<DataTransaction> ExecuteAsync(
            Entity entity, DataTransaction transaction = null,
            IDictionary<string, object> inputValues = null, object context = null,
            bool force = false)
        {
            return ExecuteAsync(new[] { entity }, transaction, inputValues, context, force);
        }

        public Task<DataTransaction> ExecuteAsync(
            IEnumerable<Entity> entities, DataTransaction transaction = null,
            IDictionary<string, object> inputValues = null, object context = null,
            bool force = false)
        {
            Entity entity = entities.FirstOrDefault();
            if (entity == null)
                return null;
            if (transaction == null)
                transaction = entity.Transaction;

            return Task.Run(() => _provider.Execute(this, entities, transaction, inputValues, context, force));
        }

        public bool ValidateEntity(Entity entity, bool throwOnError = true)
        {
            if (entity == null)
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(entity));
                return false;
            }

            return _provider.ValidateEntities(new[] { entity }, throwOnError);
        }

        public bool ValidateEntities(IEnumerable<Entity> entities, bool throwOnError = true)
        {
            if (entities == null)
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(entities));
                return false;
            }

            return _provider.ValidateEntities(entities, throwOnError);
        }

        public override string ToString()
        {
            return CommandText;
        }
    }
}
