using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lucene.Net.Analysis;

namespace Lucene.Linq.Expressions {

    /// <summary>
    /// Thread safe type cache for generic types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypeCache<T> where T:class {
        
        protected readonly Dictionary<Type, T> dictionary;
        protected readonly ReaderWriterLockSlim dictionaryLock;

        protected TypeCache() {
            dictionary = new Dictionary<Type, T>();
            dictionaryLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        internal T this[Type t] {
            get {
                if (t == null)
                    return DefaultInstance();

                using (dictionaryLock.UpgradableReadLock()){


                    if (dictionary.ContainsKey(t)) {
                        return dictionary[t];
                    } else {
                        
                        using (dictionaryLock.WriteLock()) {
                            var item = ActivateInstance(t);
                            dictionary.Add(t,item);
                            return item;
                        }
                    }
                }
            }

            set {
                using (dictionaryLock.WriteLock()) {

                    if (value == null)
                        throw new ArgumentException("value");

                    if (dictionary.ContainsKey(t)) {
                        dictionary[t] = value;
                    } else {
                        dictionary.Add(t, value);
                    }
                }
            }
        }

        protected virtual T ActivateInstance(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var result = Activator.CreateInstance(type);
            return result as T;
        }

        protected virtual T DefaultInstance() {
            return default(T);
        }
    }
}
