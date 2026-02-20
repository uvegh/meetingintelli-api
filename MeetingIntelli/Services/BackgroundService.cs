

using System.Linq.Expressions;
using Hangfire;
namespace MeetingIntelli.Services
{
    public class BackgroundService : IBackgroundService
    {
        public string EnqueueJob<T>(Expression<Func<T, Task>> methodCall)
        {
            return Hangfire.BackgroundJob.Enqueue(methodCall);
            
        }

      
    }
}
