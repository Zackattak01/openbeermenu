using Serilog;

namespace OpenBeerMenu.Types
{
   public class DelayedTask
   {
        private readonly Func<Task> _taskToDelay;
        private readonly int _delay;
        
        private bool _shouldCancel = false;

        private CancellationTokenSource _cts;

        public bool IsComplete { get; private set; } = false;

        public DelayedTask(Func<Task> taskToDelay, int delay)
        {
            _taskToDelay = taskToDelay;
            _delay = delay;

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
                    Console.WriteLine($"Delaying for {_delay} ms");
                    await Task.Delay(_delay, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    System.Console.WriteLine($"Caught with {_shouldCancel}");
                    if (_shouldCancel)
                    {
                        _shouldCancel = false;
                        return;
                    }
                }
            } while (_cts.IsCancellationRequested);
            
            try
            {

                Console.WriteLine($"Executing task");
                IsComplete = true;
                await _taskToDelay();
                _cts.Dispose();
            }
            catch (Exception e)
            {
                Log.Logger.Error("A delayed task encountered an error\n{0}", e);
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
