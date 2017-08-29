using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NightlyCode.DB.Entities {

    /// <summary>
    /// buffers entities for a key to reduce loading operations to database
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class EntityCache<TEntity, TKey> {
        readonly Func<TKey, Expression<Predicate<TEntity>>> loader;
        readonly EntityManager entitymanager;
        readonly Dictionary<TKey, TEntity> cache = new Dictionary<TKey, TEntity>();

        /// <summary>
        /// creates a new entity cache
        /// </summary>
        /// <param name="entitymanager"></param>
        /// <param name="loader"></param>
        public EntityCache(EntityManager entitymanager, Func<TKey, Expression<Predicate<TEntity>>> loader) {
            this.loader = loader;
            this.entitymanager = entitymanager;
        }

        /// <summary>
        /// get entity for key
        /// </summary>
        /// <param name="key">key for which to load entity</param>
        /// <param name="customcreator">creator which is used when no entity is found in db</param>
        /// <returns>entity for the key</returns>
        public TEntity GetEntity(TKey key, Func<TEntity> customcreator=null) {
            TEntity entity;
            if(!cache.TryGetValue(key, out entity)) {
                entity = entitymanager.LoadEntities<TEntity>().Where(loader(key)).Execute().FirstOrDefault();
                if(entity == null && customcreator != null)
                    entity = customcreator();
                cache[key] = entity;
            }
            return entity;
        }
    }
}