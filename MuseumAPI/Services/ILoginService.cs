using MuseumAPI.Model;

namespace MuseumAPI.Services
{
    public interface ILoginService
    {
        LoginResponseDto Login(LoginRequestDto request);

    }
}
