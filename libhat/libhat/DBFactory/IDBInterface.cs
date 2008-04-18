using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace libhat.DBFactory {
    public interface IDBFactory : IDisposable {
        T LookupFirst<T>( ICondition condition ) where T :class, IEntity;
        IList<T> Lookup<T>( ICondition condition ) where T :class, IEntity;

        void Save<T>( params T[] items ) where T :class, IEntity;
        void Save<T>( IEnumerable<T> items ) where T :class, IEntity;

        void Delete<T>( params T[] items ) where T :class, IEntity;
        void Delete<T>( IEnumerable<T> items ) where T :class, IEntity;

        bool IsInitialized{ get; }
    }

    public interface ICondition {
        string Name{ get; }
    }

    public interface IEntity {
        string Code{ get; set;}
    }
}