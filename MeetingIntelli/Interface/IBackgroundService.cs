using System.Linq.Expressions;
using Hangfire;
namespace MeetingIntelli.Interface;

public interface IBackgroundService
{
    string EnqueueJob<T>(Expression<Func<T, Task>> methodCall);
   

}
