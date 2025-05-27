// UberEatsBackend/Services/IStorageService.cs - ACTUALIZADO
using System.Threading.Tasks;

namespace UberEatsBackend.Services
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(string base64File, string fileName, string folder = "general");
        Task<string> SaveFileAsync(string base64File, string fileName); // Backward compatibility
        Task DeleteFileAsync(string fileUrl);
    }
}