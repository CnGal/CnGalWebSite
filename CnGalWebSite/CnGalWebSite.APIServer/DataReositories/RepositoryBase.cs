using CnGalWebSite.APIServer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.DataReositories
{
    /// <summary>
    /// 默认仓储的通用功能实现，用于所有领域模型
    /// </summary>
    /// <typeparam name="Tentity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public class RepositoryBase<Tentity, TPrimaryKey> : IRepository<Tentity, TPrimaryKey> where Tentity : class
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        protected readonly AppDbContext _dbContext;

        /// <summary>
        /// 通过泛型，从数据库上下文中获取领域模型
        /// </summary>
        public virtual DbSet<Tentity> Table => _dbContext.Set<Tentity>();

        public RepositoryBase(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Tentity> GetAll()
        {
            return Table.AsQueryable();
        }

        public List<Tentity> GetAllList()
        {
            return GetAll().ToList();
        }

        public async Task<List<Tentity>> GetAllListAsync()
        {
            return await GetAll().ToListAsync();
        }

        public List<Tentity> GetAllList(Expression<Func<Tentity, bool>> predicate)
        {
            return GetAll().Where(predicate).ToList();
        }

        public async Task<List<Tentity>> GetAllListAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().Where(predicate).ToListAsync();
        }

        public Tentity Single(Expression<Func<Tentity, bool>> predicate)
        {
            return GetAll().Single(predicate);
        }

        public async Task<Tentity> SingleAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().SingleAsync(predicate);
        }

        public Tentity FirstOrDefault(Expression<Func<Tentity, bool>> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }

        public async Task<Tentity> FirstOrDefaultAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().FirstOrDefaultAsync(predicate);
        }

        public Tentity Insert(Tentity entity)
        {
            var newEntity = Table.Add(entity).Entity;
            Save();

            return newEntity;
        }
        public async Task<Tentity> InsertAsync(Tentity entity)
        {
            var entityEntity = await Table.AddAsync(entity);
            await SaveAsync();

            return entityEntity.Entity;
        }

        public Tentity Update(Tentity entity)
        {
            RequireTracked(entity);
            Save();
            return entity;
        }

        public async Task<Tentity> UpdateAsync(Tentity entity)
        {
            RequireTracked(entity);
            await SaveAsync();
            return entity;
        }

        public Tentity UpdateFields(Tentity entity, params Expression<Func<Tentity, object>>[] fields)
        {
            MarkFields(entity, fields);
            Save();
            return entity;
        }

        public async Task<Tentity> UpdateFieldsAsync(Tentity entity, params Expression<Func<Tentity, object>>[] fields)
        {
            MarkFields(entity, fields);
            await SaveAsync();
            return entity;
        }

        private void RequireTracked(Tentity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (_dbContext.Entry(entity).State == EntityState.Detached)
                throw new InvalidOperationException($"{typeof(Tentity).Name} is detached. Load a tracked entity or use UpdateFields with explicit properties.");
        }

        private void MarkFields(Tentity entity, Expression<Func<Tentity, object>>[] fields)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (fields is null || fields.Length == 0)
                throw new ArgumentException("At least one property must be specified.", nameof(fields));
            var entry = _dbContext.Entry(entity);
            // Attach only the root, never a detached navigation graph supplied by a caller.
            if (entry.State == EntityState.Detached) entry.State = EntityState.Unchanged;
            foreach (var field in fields)
            {
                var body = field.Body is UnaryExpression unary ? unary.Operand : field.Body;
                if (body is not MemberExpression member || member.Expression != field.Parameters[0])
                    throw new ArgumentException("Only direct scalar properties can be updated.", nameof(fields));
                var property = entry.Property(member.Member.Name);
                if (property.Metadata.IsPrimaryKey())
                    throw new ArgumentException("Primary keys cannot be updated.", nameof(fields));
                property.IsModified = true;
            }
        }

        public void Delete(Tentity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
            Save();
        }

        public async Task DeleteAsync(Tentity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
            await SaveAsync();
        }

        public void Delete(Expression<Func<Tentity, bool>> predicate)
        {
            foreach (var entity in GetAll().Where(predicate).ToList())
            {
                Delete(entity);
            }
        }

        public async Task DeleteAsync(Expression<Func<Tentity, bool>> predicate)
        {
            foreach (var entity in GetAll().Where(predicate).ToList())
            {
                await DeleteAsync(entity);
            }
        }

        public int Count()
        {
            return GetAll().Count();
        }
        public async Task<int> CountAsync()
        {
            return await GetAll().CountAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().AnyAsync(predicate);
        }

        public int Count(Expression<Func<Tentity, bool>> predicate)
        {
            return GetAll().Where(predicate).Count();
        }
        public async Task<int> CountAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().Where(predicate).CountAsync();
        }

        public long LongCount()
        {
            return GetAll().LongCount();
        }

        public async Task<long> LongCountAsync()
        {
            return await GetAll().LongCountAsync();
        }

        public long LongCount(Expression<Func<Tentity, bool>> predicate)
        {
            return GetAll().Where(predicate).LongCount();
        }
        public async Task<long> LongCountAsync(Expression<Func<Tentity, bool>> predicate)
        {
            return await GetAll().Where(predicate).LongCountAsync();
        }

        public void Clear()
        {
            _dbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// 检查实体是否处于跟踪状态 如果是 则返回 如果不是 则添加跟踪状态
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void AttachIfNot(Tentity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var entry = _dbContext.Entry(entity);
            if (entry.State == EntityState.Detached) entry.State = EntityState.Unchanged;
        }

        protected void Save()
        {
            //调用数据库上下文保存数据
            _dbContext.SaveChanges();
        }

        protected async Task SaveAsync()
        {
            //调用数据库上下文保存数据
            await _dbContext.SaveChangesAsync();
        }


    }
}
