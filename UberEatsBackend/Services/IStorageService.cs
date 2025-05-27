// UberEatsBackend/Services/IStorageService.cs
using System.Threading.Tasks;

namespace UberEatsBackend.Services
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(string base64File, string fileName);
        Task DeleteFileAsync(string fileUrl);
    }
}
