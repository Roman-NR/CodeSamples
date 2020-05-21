using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace CodeSamples.TaskExecutor
{
    /// <summary>
    /// Планировщик заданий с собственным пулом потоков (собственный пул сделан сделан для того, чтобы не уменьшать стандартный)
    /// </summary>
    internal class TaskExecutor : ITaskExecutor
    {
        private ISchedulerItemRepository _repository;
        private ILogger _logger;
        private List<Thread> _threads = new List<Thread>(Environment.ProcessorCount * 10);
        private Queue<TaskInfo> _queue = new Queue<TaskInfo>(_threads.Capacity * 10);
        private AutoResetEvent _taskAdded = new AutoResetEvent(false);
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private volatile int _runningCount = 0;

        public TaskExecutor(ISchedulerItemRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void Enqueue(TaskInfo task)
        {
            lock (_queue)
                _queue.Enqueue(task);

            _taskAdded.Set();

            lock (_threads)
            {
                if (_runningCount == _threads.Count && _threads.Count < _threads.Capacity)
                {
                    int index = _threads.Count;
                    Thread thread = new Thread(ProcessThread);
                    thread.IsBackground = true;
                    thread.Name = "TaskProcessThread_" + index;
                    thread.Start(index);
                    _threads.Add(thread);
                }
            }
        }

        public void Stop()
        {
            _cancel.Cancel();
            for (int i = 0; i < 10 && _runningCount > 0; ++i)
                Thread.Sleep(200);
            if (_runningCount > 0)
            {
                foreach (Thread thread in _threads)
                    thread.Abort();
            }
        }

        public void ProcessThread(object state)
        {
            while (!_cancel.IsCancellationRequested)
            {
                TaskInfo task = null;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        task = _queue.Dequeue();
                }

                if (task == null)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { _taskAdded, _cancel.Token.WaitHandle });
                    continue;
                }

                if (_cancel.IsCancellationRequested)
                    return;

                _runningCount++;
                try
                {
                    DateTime? runAgainOnError = DateTime.Now.AddHours(1);

                    SchedulerItem item = _repository.GetSchedulerItem(task.SchedulerItemGuid);

                    TaskCompleteInfo completeInfo = new TaskCompleteInfo();
                    completeInfo.Id = task.Id;
                    completeInfo.StartDateTime = DateTime.Now;
                    if (item != null)
                    {
                        SchedulerItemActionContext context = new SchedulerItemActionContext(
                            item, completeInfo.StartDateTime, task.DataXml, (ex) => _logger.LogError(ex, sourceId: task.SchedulerItemGuid));

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        try
                        {
#if DEBUG
                            //System.Windows.MessageBox.Show("TaskExecutor. Attach Debugger Now.");
#endif

                            StringBuilder result = new StringBuilder();

                            foreach (SchedulerItemAction action in item.Actions)
                            {
                                string text = action.Execute(context);
                                if (!string.IsNullOrEmpty(text))
                                {
                                    if (result.Length > 0)
                                        result.AppendLine();
                                    result.Append(text);
                                }
                            }
                            foreach (SchedulerItemEvent @event in item.Events)
                                @event.OnExecuted(context);

                            completeInfo.Result = result.ToString();
                            completeInfo.NextExecuteDateTime = context.NextExecuteDateTime;
                            completeInfo.NextExecuteDataXml = context.NextExecuteDataXml;
                            completeInfo.DeleteSchedulerItem = context.DeleteSchedulerItem;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, sourceId: task.SchedulerItemGuid);
                            completeInfo.Result = e.Message;
                            completeInfo.IsError = true;
                            completeInfo.NextExecuteDateTime = runAgainOnError;
                            completeInfo.NextExecuteDataXml = task.DataXml;
                        }
                        finally
                        {
                            stopwatch.Stop();
                            completeInfo.Duration = stopwatch.Elapsed;
                        }
                    }

                    if (_cancel.IsCancellationRequested)
                        return;

                    _repository.OnTaskComplete(completeInfo);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, sourceId: task.SchedulerItemGuid);
                }
                finally
                {
                    _runningCount--;
                }
            }
        }
    }
}
