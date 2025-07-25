﻿namespace FinalProject_ITI.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetQuery();
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(int id);
    Task Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChanges();
}
