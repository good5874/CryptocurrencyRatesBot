using Quartz;
using Quartz.Impl;

namespace CryptocurrencyRatesBot.Web.Services
{
    public class SchedulerService
    {
        public static void StartUpdateRates()
        {
            IJobDetail cryptorankJob = JobBuilder.Create<CryptorankJob>()
                .WithIdentity("CryptorankJob")
                .Build();
            
            IJobDetail notificationJob = JobBuilder.Create<NotificationJob>()
                .WithIdentity("NotificationJob")
                .Build();

            ITrigger cryptorankTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever())
                .Build();
            ITrigger notificationTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever())
                .Build();

            IScheduler scheduler = new StdSchedulerFactory().GetScheduler().Result;
            scheduler.ScheduleJob(cryptorankJob, cryptorankTrigger);
            scheduler.ScheduleJob(notificationJob, notificationTrigger);
            scheduler.Start();
        }
    }
}
