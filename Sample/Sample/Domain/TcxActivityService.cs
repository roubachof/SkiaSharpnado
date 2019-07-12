using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using TcxTools;

namespace Sample.Domain
{
    public class TcxActivityService : ITcxActivityService
    {
        private const string RuntasticFileFormat = "runtastic_{0}.tcx";
        private const string RuntasticFileDateFormat = "runtastic_{0:yyyyMMdd_HHmm}.tcx";
        private const string RuntasticDateFormat = "yyyyMMdd_HHmm";

        private readonly List<Activity> _activities = new List<Activity>();

        private static readonly string[] Ids = new string[]
            {
                "20190616_0854",
                "20190412_1605",
                "20181112_2350",
            };

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            if (_activities.Count > 0)
            {
                return _activities;
            }

            foreach (var activityResourceName in Embedded.GetAllDomainResources())
            {
                _activities.Add(await GetActivityByResourceName(activityResourceName));
            }

            //foreach (string id in Ids)
            //{
            //    _activities.Add(await GetActivityAsync(id));
            //}

            return _activities;
        }

        public Task<Activity> GetActivityAsync(string id)
        {
            return Task.Run(
                () =>
                    {
                        using (var stream = Embedded.Load(string.Format(RuntasticFileFormat, id)))
                        {
                            var serializer = new XmlSerializer(typeof(TrainingCenterDatabase));
                            var @object = serializer.Deserialize(stream);

                            var database = (TrainingCenterDatabase)@object;
                            return database.Activities.Activity[0];
                        }
                    });
        }

        private Task<Activity> GetActivityByResourceName(string resourceName)
        {
            return Task.Run(
                () =>
                    {
                        using (var stream = Embedded.LoadWithFullName(resourceName))
                        {
                            var serializer = new XmlSerializer(typeof(TrainingCenterDatabase));
                            var @object = serializer.Deserialize(stream);

                            var database = (TrainingCenterDatabase)@object;
                            return database.Activities.Activity[0];
                        }
                    });
        }
    }
}
