using System;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    public interface ILoggerService
    {
        Task Fatal(Exception exception);
    }
}
