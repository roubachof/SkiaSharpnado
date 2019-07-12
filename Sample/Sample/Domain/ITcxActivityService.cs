using System;
using System.Threading.Tasks;

using TcxTools;

namespace Sample.Domain
{
    public interface ITcxActivityService
    {
        Task<System.Collections.Generic.List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityAsync(string id);
    }
}