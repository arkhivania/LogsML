using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.IndexStore.Base
{
    public interface IIndexStore<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        void Load(Server.Base.IIndex<TKey, TValue> index);
        void Store(Server.Base.IIndex<TKey, TValue> index);
    }
}
