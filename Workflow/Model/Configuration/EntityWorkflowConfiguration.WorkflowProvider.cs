using System;
using System.Collections.Generic;
using System.Linq;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class WorkflowProvider : IEntityWorkflowProvider
        {
            public Guid Guid { get; set; }
            public EntitySetInfo EntitySet { get; set; }
            public Guid StageListGuid { get; set; }
            public Guid StagePropertyGuid { get; set; }
            public IEntityWorkflowTypeRule[] Types { get; set; }
            public IStageChangeHistoryConfiguration HistoryConfiguration { get; set; }
            public HashSet<Guid> ShowInLinks { get; set; }
            public string Name { get; set; }
            public string CodeName { get { return null; } }
            public string PropertyName { get; set; }
            public string PropertyCodeName { get { return null; } }
            public EntityWorkflowScheme Scheme { get { return null; } }

            public bool GetEntityStageId(Entity entity, DataTransaction transaction, out object stageId)
            {
                stageId = null;
                if (entity == null)
                    return false;

                if (StageListGuid != Guid.Empty)
                {
                    ValueListPropertyFormat format = entity.Type.Properties.Find(StagePropertyGuid)?.Format as ValueListPropertyFormat;
                    if (format?.ValueList?.Guid != StageListGuid)
                        return false;
                }

                if (!IsTypeSupported(entity.Type))
                    return false;

                return entity.TryGetValue(StagePropertyGuid, out stageId);
            }

            public void SetEntityStageId(Entity entity, object stageId, string action, IDictionary<string, object> inputValues, TransitionLogHandler onLog)
            {
                entity.SetValue(StagePropertyGuid, stageId, true, true);
                if (HistoryConfiguration != null)
                {
                    EntitySet historySet = entity.Connection.GetEntitySet(HistoryConfiguration.EntitySetGuid)?.CreateEntitySet();
                    if (historySet != null)
                    {
                        Entity historyItem = HistoryConfiguration.EntityTypeGuid == Guid.Empty ?
                            historySet.CreateEntity(entity.Transaction) :
                            historySet.CreateEntity(entity.Transaction, HistoryConfiguration.EntityTypeGuid);

                        historyItem[HistoryConfiguration.StagePropertyGuid] = stageId;
                        historyItem.Name = action;
                        if (HistoryConfiguration.DateTimePropertyGuid != Guid.Empty)
                            historyItem[HistoryConfiguration.DateTimePropertyGuid] = DateTime.Now;
                        if (HistoryConfiguration.CommentPropertyGuid != Guid.Empty && inputValues.TryGetValue("Comment", out object comment))
                            historyItem[HistoryConfiguration.CommentPropertyGuid] = comment?.ToString().Trim() ?? string.Empty;
                        if (HistoryConfiguration.ResponsibleLinkGuid != Guid.Empty)
                        {
                            historyItem.SetLinkedEntity(HistoryConfiguration.ResponsibleLinkGuid,
                                entity.Connection.Workspace.GetUser(), entity.Transaction);
                        }
                        historyItem.SetLinkedEntity(HistoryConfiguration.OwnerLinkGuid, entity, entity.Transaction);

                        onLog?.Invoke(entity, historyItem, inputValues);
                    }
                }
            }

            public bool IsTypeSupported(EntityType type)
            {
                return Types.Length == 0 || Types.Any(t => t.Check(type));
            }

            public bool IsCommandVisibleInLinks
            {
                get { return ShowInLinks.Count > 0; }
            }

            public bool IsCommandVisible(Guid linkGuid)
            {
                return ShowInLinks.Contains(linkGuid);
            }

            public DataTransaction ForceChangeStage(IEnumerable<Entity> entities, EntityWorkflowStage stage, DataTransaction dataTransaction)
            {
                using (DataTransactionSwitch transaction = new DataTransactionSwitch(EntitySet.Connection, dataTransaction))
                {
                    foreach (Entity entity in entities)
                    {
                        if (!GetEntityStageId(entity, transaction, out object stageId))
                            throw new ModelException(ErrorLevel.Information, "Нельзя сменить стадию объекта на {0}", stage);

                        if (!entity.OnWorkflowStageChanging(stage, null, null))
                            return null;

                        entity.Edit(transaction);
                        if (entity.EntitySet.Extension != null && !entity.EntitySet.Extension.OnWorkflowStageChanging(entity, stage, null, null, null))
                            return null;

                        SetEntityStageId(entity, stage.Id, "Назначение стадии", null, null);
                    }

                    transaction.Commit();

                    return transaction.Transaction;
                }
            }

            public ConditionItemData GetConditionItem(List<Guid> links, SearchCondition condition)
            {
                return new PropertyItemData() { Links = links, PropertyGuid = StagePropertyGuid };
            }

            public IEnumerable<EntityWorkflowStageChange> GetStageChanges(EntityWorkflow workflow, Entity entity, IEnumerable<WorkflowStageChangeData> dataList)
            {
                if (HistoryConfiguration == null || !GetEntityStageId(entity, null, out _))
                    yield break;

                EntitySet historySet = entity.Connection.GetEntitySet(HistoryConfiguration.EntitySetGuid)?.CreateEntitySet();
                if (historySet != null)
                {
                    EntityLoadParameters loadParameters = new EntityLoadParameters(historySet);
                    if (HistoryConfiguration.ResponsibleLinkGuid != Guid.Empty)
                        loadParameters.AddLink(HistoryConfiguration.ResponsibleLinkGuid);
                    loadParameters.Filter = new EntitySetFilter(historySet.GetInfo());
                    loadParameters.Filter.And(HistoryConfiguration.OwnerLinkGuid, "=", entity.Guid);
                    if (HistoryConfiguration.EntityTypeGuid != Guid.Empty)
                        loadParameters.Filter.And(SystemPropertyItem.Type, "=", HistoryConfiguration.EntityTypeGuid);

                    IEntityCollection<Entity> historyItems = historySet.GetEntities(loadParameters);
                    EntityCollection<LinkEntity> responsibleLinks = HistoryConfiguration.ResponsibleLinkGuid != Guid.Empty ? 
                        historyItems.GetLinks(HistoryConfiguration.ResponsibleLinkGuid) : null;

                    foreach (Entity historyItem in historyItems)
                    {
                        EntityWorkflowStage stage = workflow.GetStage(historyItem[HistoryConfiguration.StagePropertyGuid]);
                        Guid userGuid = responsibleLinks == null ? 
                            historyItem.CreatedBy : (responsibleLinks.FindOne<Entity>(historyItem.Guid)?.Guid ?? Guid.Empty);
                        DateTime changeTime = HistoryConfiguration.DateTimePropertyGuid == Guid.Empty ? 
                            historyItem.Created.GetValueOrDefault() : (DateTime)historyItem[HistoryConfiguration.DateTimePropertyGuid];
                        string comment = HistoryConfiguration.CommentPropertyGuid == Guid.Empty ?
                            string.Empty : (historyItem[HistoryConfiguration.CommentPropertyGuid]?.ToString() ?? string.Empty);
                        yield return new EntityWorkflowStageChange(workflow, stage, null, userGuid, changeTime, historyItem.Name, comment);
                    }
                }
            }
        }
    }
}
