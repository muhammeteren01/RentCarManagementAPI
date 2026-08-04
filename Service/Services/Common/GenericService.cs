using System.Linq.Expressions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;

namespace Service.Services.Common;

public class GenericService<T> : IGenericService<T> where T : class
{
    protected readonly IGenericRepository<T> Repository;
    protected readonly IUnitOfWork UnitOfWork;

    public GenericService(IGenericRepository<T> repository, IUnitOfWork unitOfWork)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await Repository.GetByIdAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await Repository.FindAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await Repository.AddAsync(entity);
        await UnitOfWork.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync();
    }

    public virtual async Task RemoveAsync(T entity)
    {
        Repository.Remove(entity);
        await UnitOfWork.SaveChangesAsync();
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await Repository.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return await Repository.CountAsync(predicate);
    }
}
