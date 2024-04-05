using Serilog;
using ILogger = Serilog.ILogger;

namespace OpenBeerMenu.Types
{
   public class DelayedTask
   {
        private readonly Func<Task> _taskToDelay;
        private readonly int _delay;
        private readonly ILogger _logger;
        
        private bool _shouldCancel = false;

        private CancellationTokenSource _cts;

        public bool IsComplete { get; private set; } = false;

        public DelayedTask(Func<Task> taskToDelay, int delay)
        {
            _taskToDelay = taskToDelay;
            _delay = delay;
            _logger = Log.Logger.ForContext("SourceContext", typeof(DelayedTask).Name);
            

            _ = DelayLoop();
        }

        private async Task DelayLoop()
        {
            do 
            {
                if (_cts is not null)
                    _cts.Dispose();

                _cts = new CancellationTokenSource();
                try
                {
                    _logger.Information($"Delaying for {_delay} ms");
                    await Task.Delay(_delay, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    if (_shouldCancel)
                    {
                        _logger.Information("Cancelling task");
                        _shouldCancel = false;
                        IsComplete = true;
                        return;
                    }
                }
            } while (_cts.IsCancellationRequested);
            
            try
            {

                _logger.Information($"Executing task");
                IsComplete = true;
                await _taskToDelay();
                _cts.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error("A delayed task encountered an error\n{0}", e);
            }            
        }

        public void Postpone()
        {
            if (_cts is not null)
                _cts.Cancel();
        }

        public void Cancel()
        {
            _shouldCancel = true;
            if (_cts is not null)
                _cts.Cancel();
        }
    }
}
