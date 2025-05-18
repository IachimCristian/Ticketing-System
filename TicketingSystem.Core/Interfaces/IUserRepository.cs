using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IUserRepository<T> : IRepository<T> where T : User
    {
        Task<T> GetByEmailAsync(string email);
        Task<T> GetByUsernameAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
    }
} 