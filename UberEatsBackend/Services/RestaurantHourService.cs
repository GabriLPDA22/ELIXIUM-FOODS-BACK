// UberEatsBackend/Services/RestaurantHourService.cs
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Restaurant;
using AutoMapper;

namespace UberEatsBackend.Services
{
    public class RestaurantHourService : IRestaurantHourService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RestaurantHourService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RestaurantHourDto>> GetRestaurantHoursAsync(int restaurantId)
        {
            try
            {
                // ARREGLADO: Primero obtener los datos, luego ordenar en memoria
                var hours = await _context.RestaurantHours
                    .Where(r => r.RestaurantId == restaurantId)
                    .ToListAsync(); // Traer a memoria primero

                // Ahora ordenar en memoria usando el método personalizado
                var orderedHours = hours
                    .OrderBy(r => GetDayOrder(r.DayOfWeek))
                    .ToList();

                return _mapper.Map<List<RestaurantHourDto>>(orderedHours);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener horarios del restaurante {restaurantId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> BulkUpdateRestaurantHoursAsync(int restaurantId, BulkUpdateRestaurantHoursDto hoursDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Eliminar horarios existentes
                var existingHours = await _context.RestaurantHours
                    .Where(rh => rh.RestaurantId == restaurantId)
                    .ToListAsync();

                if (existingHours.Any())
                {
                    _context.RestaurantHours.RemoveRange(existingHours);
                }

                // Crear nuevos horarios - AHORA FUNCIONA CON TU DTO ACTUALIZADO
                var newHours = hoursDto.Hours.Select(h => new RestaurantHour
                {
                    RestaurantId = restaurantId,
                    DayOfWeek = h.DayOfWeek,        // string a string directo
                    IsOpen = h.IsOpen,              // bool a bool directo
                    OpenTime = TimeSpan.Parse(h.OpenTime),   // string "10:00" a TimeSpan
                    CloseTime = TimeSpan.Parse(h.CloseTime)  // string "22:00" a TimeSpan
                }).ToList();

                await _context.RestaurantHours.AddRangeAsync(newHours);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al actualizar horarios: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsRestaurantOpenAsync(int restaurantId)
        {
            return await IsRestaurantOpenAtTimeAsync(restaurantId, DateTime.Now);
        }

        public async Task<bool> IsRestaurantOpenAtTimeAsync(int restaurantId, DateTime dateTime)
        {
            try
            {
                var dayOfWeekString = GetDayName((int)dateTime.DayOfWeek);
                var currentTime = dateTime.TimeOfDay;

                var todayHours = await _context.RestaurantHours
                    .Where(rh => rh.RestaurantId == restaurantId && rh.DayOfWeek == dayOfWeekString)
                    .FirstOrDefaultAsync();

                if (todayHours == null || !todayHours.IsOpen)
                    return false;

                // Verificar si está dentro del horario
                if (todayHours.CloseTime > todayHours.OpenTime)
                {
                    // Horario normal (no cruza medianoche)
                    return currentTime >= todayHours.OpenTime && currentTime <= todayHours.CloseTime;
                }
                else
                {
                    // Horario que cruza medianoche (ej: 22:00 - 02:00)
                    return currentTime >= todayHours.OpenTime || currentTime <= todayHours.CloseTime;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar si el restaurante está abierto: {ex.Message}", ex);
            }
        }

        public async Task<string> GetRestaurantStatusAsync(int restaurantId)
        {
            try
            {
                var now = DateTime.Now;
                var currentDayOfWeekString = GetDayName((int)now.DayOfWeek);
                var currentTime = now.TimeOfDay;

                // Obtener horarios de hoy
                var todayHours = await _context.RestaurantHours
                    .Where(rh => rh.RestaurantId == restaurantId && rh.DayOfWeek == currentDayOfWeekString)
                    .FirstOrDefaultAsync();

                if (todayHours == null || !todayHours.IsOpen)
                {
                    // Buscar el próximo día que abra
                    var nextOpenDay = await FindNextOpenDayAsync(restaurantId, (int)now.DayOfWeek);
                    return nextOpenDay != null ?
                        $"Cerrado hoy. Abre {nextOpenDay.DayOfWeek} a las {nextOpenDay.OpenTime:hh\\:mm}" :
                        "Cerrado temporalmente";
                }

                var isCurrentlyOpen = await IsRestaurantOpenAtTimeAsync(restaurantId, now);

                if (isCurrentlyOpen)
                {
                    return $"Abierto hasta las {todayHours.CloseTime:hh\\:mm}";
                }
                else
                {
                    if (currentTime < todayHours.OpenTime)
                    {
                        return $"Abre hoy a las {todayHours.OpenTime:hh\\:mm}";
                    }
                    else
                    {
                        // Ya cerró hoy, buscar próximo día
                        var nextOpenDay = await FindNextOpenDayAsync(restaurantId, (int)now.DayOfWeek);
                        return nextOpenDay != null ?
                            $"Abre {nextOpenDay.DayOfWeek} a las {nextOpenDay.OpenTime:hh\\:mm}" :
                            "Cerrado temporalmente";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estado del restaurante: {ex.Message}", ex);
            }
        }

        // MÉTODO PRIVADO - Para ordenar días (adaptado a string)
        private static int GetDayOrder(string dayOfWeek)
        {
            return dayOfWeek?.ToLower() switch
            {
                "monday" => 1,
                "tuesday" => 2,
                "wednesday" => 3,
                "thursday" => 4,
                "friday" => 5,
                "saturday" => 6,
                "sunday" => 7,
                _ => 8 // Desconocido al final
            };
        }

        private async Task<RestaurantHour?> FindNextOpenDayAsync(int restaurantId, int currentDayOfWeek)
        {
            // Buscar en los próximos 7 días
            for (int i = 1; i <= 7; i++)
            {
                var nextDay = (currentDayOfWeek + i) % 7;
                var nextDayString = GetDayName(nextDay);

                var nextDayHours = await _context.RestaurantHours
                    .Where(rh => rh.RestaurantId == restaurantId &&
                                rh.DayOfWeek == nextDayString &&
                                rh.IsOpen) // Usar IsOpen en lugar de !IsClosed
                    .FirstOrDefaultAsync();

                if (nextDayHours != null)
                {
                    return nextDayHours;
                }
            }

            return null;
        }

        // Convertir int DayOfWeek a string para consultas
        private static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "sunday",
                1 => "monday",
                2 => "tuesday",
                3 => "wednesday",
                4 => "thursday",
                5 => "friday",
                6 => "saturday",
                _ => "unknown"
            };
        }
    }
}
