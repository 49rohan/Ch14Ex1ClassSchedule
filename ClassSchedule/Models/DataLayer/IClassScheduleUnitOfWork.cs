namespace ClassSchedule.Models
{
    public interface IClassScheduleUnitOfWork
    {
        IRepository<Day> Days { get; }
        IRepository<Teacher> Teachers { get; }
        IRepository<Class> Classes { get; }

        void Save();
    }
}