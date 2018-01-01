﻿using Differences.Interaction.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Differences.Interaction.Repositories
{
    public interface IRepository<TEntity> where TEntity : AggregateRoot
    {
        DbContext DbContext { get; }

        TEntity Get(int id);
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression);
        IQueryable<TEntity> Find(ISpecification<TEntity> spec);
        IQueryable<TEntity> GetAll();
        bool Exists(int id);

        TEntity Add(TEntity entity);
        int Remove(int id);
        TEntity Update(int id, TEntity entity);

        void SaveChanges();
        Task SaveChangesAsync();

        void LoadReference<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression)
            where T : class
            where TProperty : class;
    }
}
