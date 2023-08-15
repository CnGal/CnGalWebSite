using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.RobotClientX.Models.Events;
using CnGalWebSite.RobotClientX.DataRepositories;
using CnGalWebSite.RobotClientX.Models.Robots;
using CnGalWebSite.RobotClientX.Extentions;

namespace CnGalWebSite.RobotClientX.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IRepository<RobotEvent> _robotEventRepository;
        private readonly IRepository<EventExecuteInfor> _eventExecuteInforRepository;

        public EventService(IRepository<RobotEvent> robotEventRepository, IRepository<EventExecuteInfor> eventExecuteInforRepository)
        {
            _robotEventRepository = robotEventRepository;
            _eventExecuteInforRepository = eventExecuteInforRepository;
        }

        /// <summary>
        /// 获取当前时间点定时事件
        /// </summary>
        /// <returns></returns>
        public string GetCurrentTimeEvent()
        {
            var events = _robotEventRepository.GetAll().Where(s => s.Time.TimeOfDay < DateTime.Now.ToCstTime().TimeOfDay && s.Time.AddSeconds(s.DelaySecond).TimeOfDay > DateTime.Now.ToCstTime().TimeOfDay && s.IsHidden == false && s.Type == RobotEventType.FixedTime);

            var todos = new List<RobotEvent>();

            foreach (var item in events)
            {
                if (_eventExecuteInforRepository.GetAll().Any(s => (s.Id == item.Id || s.RealExecute) && s.LastRunTime.Date >= DateTime.Now.ToCstTime().Date && s.Note == item.Note))
                {
                    continue;
                }

                todos.Add(item);
            }

            if (todos.Count == 0)
            {
                return null;
            }

            todos.Random();

            RobotEvent currentEvent = null;

            foreach (var item in todos)
            {
                //查找同类型的任务
                var sameCount = _robotEventRepository.GetAll().Where(s => s.Note == item.Note && s.Time.TimeOfDay >= item.Time.TimeOfDay).Count();
                if (new Random().Next(0, sameCount) == 0)
                {
                    currentEvent = item;
                }

                //无论有没有被选择中都写入已执行中
                _eventExecuteInforRepository.Insert(new EventExecuteInfor
                {
                    Id = item.Id,
                    Note = item.Note,
                    RealExecute = currentEvent != null,
                    LastRunTime = DateTime.Now.ToCstTime(),
                });

                //确定任务后结束循环
                if (currentEvent != null)
                {
                    break;
                }

            }


            return currentEvent?.Text;

        }

        /// <summary>
        /// 获取当前时间点概率事件
        /// </summary>
        /// <returns></returns>
        public string GetProbabilityEvents()
        {
            var p = new Random().NextDouble();

            var events = _robotEventRepository.GetAll().Where(s => s.Time.TimeOfDay < DateTime.Now.ToCstTime().TimeOfDay && s.Time.AddSeconds(s.DelaySecond).TimeOfDay > DateTime.Now.ToCstTime().TimeOfDay && s.IsHidden == false && s.Type == RobotEventType.PreTime && s.Probability / 100 > p);

            var todos = new List<RobotEvent>();

            foreach (var item in events)
            {
                if (_eventExecuteInforRepository.GetAll().Any(s => s.LastRunTime.Date == DateTime.Now.ToCstTime().Date && s.Id == item.Id))
                {
                    continue;
                }

                todos.Add(item);
            }

            if (todos.Count == 0)
            {
                return null;
            }


            todos.Random();

            var temp = todos.FirstOrDefault();

            _eventExecuteInforRepository.Insert(new EventExecuteInfor
            {
                Id = temp.Id,
                Note = temp.Note,
                RealExecute = true,
                LastRunTime = DateTime.Now.ToCstTime(),
            });


            return temp.Text;
        }
    }
}
