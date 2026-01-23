using MuseumAPI.Model;

namespace MuseumAPI.Services
{
    public interface ICustomerService
    {
        Task<bool> RegisterCustomerAsync(RegisterCustomerDto dto);

    }
}
