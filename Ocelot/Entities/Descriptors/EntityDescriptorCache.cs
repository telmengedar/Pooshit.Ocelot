﻿using System;
using System.Collections.Generic;

namespace Pooshit.Ocelot.Entities.Descriptors {

    /// <summary>
    /// caches models for entities
    /// </summary>
    public class EntityDescriptorCache {
        readonly Dictionary<Type, EntityDescriptor> descriptors = new();
        readonly object modellock = new();

        /// <summary>
        /// get entitydescriptor for the specified type
        /// </summary>
        /// <typeparam name="T">type of which to get entity descriptor</typeparam>
        /// <returns>entity descriptor for specified type</returns>
        public EntityDescriptor Get<T>()
        {
            return Get(typeof(T));
        }

        /// <summary>
        /// get entitydescriptor for the specified type
        /// </summary>
        /// <param name="type">type of which to get entity descriptor</param>
        /// <returns>entity descriptor for specified type</returns>
        public EntityDescriptor Get(Type type)
        {
            lock (modellock) {
                if (!descriptors.TryGetValue(type, out EntityDescriptor descriptor))
                    descriptors[type] = descriptor = EntityDescriptor.Create(type);
                return descriptor;
            }
        }

    }
}