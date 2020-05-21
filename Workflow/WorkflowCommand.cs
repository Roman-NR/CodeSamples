using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow
{
    internal sealed class WorkflowCommand : ExtensionCommandBase
    {
        private readonly EntityWorkflowTransition _transition;

        public WorkflowCommand(EntityWorkflowTransition transition, ImageCache imageCache, Guid linkGuid = default)
        {
            _transition = transition;
            if (transition.IconImageGuid != Guid.Empty)
                Image = imageCache.GetImage(transition.IconImageGuid, transition.IconImageColor);
            LinkGuid = linkGuid;
        }

        public override string Text
        {
            get { return _transition.CommandText; }
        }

        public override ImageSource Image
        {
            get;
        }

        public Guid LinkGuid { get; }
        public bool ShowSeparator { get; set; }

        public override CommandPlacement Placement
        {
            get { return ShowSeparator ? _transition.CommandPlacement | CommandPlacement.NewGroup : _transition.CommandPlacement; }
        }

        public override KeyGesture Gesture
        {
            get { return _transition.Gesture; }
        }

        public override string Tooltip
        {
            get { return _transition.Tooltip; }
        }

        public override bool CanExecute(IEntitySetExtensionContext context)
        {
            SelectionState selectionState = context.SelectionState;

            if (selectionState.IsEmpty())
                return false;

            if (!_transition.MultipleSelectionEnabled && selectionState.IsMultiple())
                return false;

            IList<IEntityPresenter> selection = context.GetSelectedItems();

            if (selection.Count == 0 || (!_transition.MultipleSelectionEnabled && selection.Count != 1))
                return false;

            return WorkflowService.CanExecute(_transition, selection.SelectMany(item => GetEntities(item)), context);
        }

        public override async void Execute(IEntitySetExtensionContext context)
        {
            IList<IEntityPresenter> items = context.GetSelectedItems();
            HashSet<Guid> guids = new HashSet<Guid>();

            EntitySet entitySet = null;
            foreach (IEntityPresenter item in items)
            {
                foreach (Entity entity in GetEntities(item))
                {
                    if (entity == null)
                        return;

                    if (entitySet == null)
                        entitySet = entity.EntitySet;
                    else if (entitySet.Scheme.Guid != entity.EntitySetGuid)
                        return;

                    guids.Add(entity.Guid);
                }
            }

            var saveResult = await context.PropertiesView?.SaveAsync();

            if (guids.Count == 0)
                return;

            List<Entity> entities = new List<Entity>(guids.Count);
            foreach (Entity entity in entitySet.Find(guids, context.EntitiesView?.EntityLoadParameters) ?? Enumerable.Empty<Entity>())
            {
                if (guids.Remove(entity.Guid))
                    entities.Add(entity);
                else
                    throw new ApplicationException();
            }

            if (guids.Count > 0)
                throw new ModelException(ErrorLevel.Warning, "Выбранные объекты были изменены или удалены. Обновите окно и попробуйте сменить стадию ещё раз.");

            DataTransaction transaction = null;
            try
            {
                transaction = await WorkflowService.ChangeStageAsync(_transition, entities, saveResult?.ExtensionContext ?? context);
                if (transaction != null)
                    context.Update(transaction);
            }
            finally
            {
                if (transaction == null)
                {
                    if (context.EntitiesView == null)
                        context.PropertiesView?.Refresh();
                    else
                        context.EntitiesView.Update(entities);
                }
            }
        }

        private IEnumerable<Entity> GetEntities(IEntityPresenter item)
        {
            if (LinkGuid == Guid.Empty)
            {
                yield return item?.Entity;
            }
            else
            {
                EntityTypeLink link = item.Entity?.Type.Links.Find(LinkGuid);
                if (link == null)
                    yield break;

                if (link.Link.Type.IsToOne())
                {
                    yield return item.Entity.GetLink<Entity>(LinkGuid);
                }
                else
                {
                    foreach (LinkEntity linkedEntity in item.Entity.GetLinks(LinkGuid))
                        yield return linkedEntity.SecondEntity;
                }
            }
        }
    }
}
