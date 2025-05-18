using System.Threading.Tasks;

namespace TicketingSystem.Core.Interfaces
{
    public interface ICommand
    {
        Task ExecuteAsync();
        Task UndoAsync();
    }
} 