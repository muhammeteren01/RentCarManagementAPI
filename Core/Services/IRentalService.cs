using Core.DTOs.Rentals;
using Core.Entities;
using Core.Enums;

namespace Core.Services;

public interface IRentalService : IGenericService<Rental>
{
    Task<RentalResponse> CreateAsync(Guid userId, CreateRentalRequest request);
    Task<RentalResponse> ExtendAsync(Guid rentalId, Guid requesterId, UserRole role, ExtendRentalRequest request);
    Task<RentalResponse> ReturnAsync(Guid rentalId, Guid requesterId, UserRole role, ReturnRentalRequest request);
    Task<RentalResponse> CancelAsync(Guid rentalId, Guid requesterId, UserRole role);
    Task<RentalResponse> GetByIdAsync(Guid rentalId, Guid requesterId, UserRole role);
    Task<IEnumerable<RentalResponse>> GetHistoryAsync(Guid requesterId, UserRole role);
}
