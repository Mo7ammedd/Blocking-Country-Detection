namespace BD.Core.Interfaces.Services;

using BD.Core.Models;
using System.Threading.Tasks;

public interface IIpGeolocationService
{
    Task<IpGeolocationData?> GetIpGeolocationDataAsync(string ipAddress);
}