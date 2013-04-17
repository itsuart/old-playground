using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Extensions;

namespace Lib.Collections
{
    public class HashMap<TKey,TValue> : IDictionary<TKey,TValue>
    {
        private readonly Func<TKey, int> _getHash;
        private readonly Func<TKey, TKey, bool> _equal;

        private readonly TKey _emptyKey;

        private TKey[] _keys;
        private TValue[] _values;
        private int _totalElements;
        private int _capacity;

        private const int DEFAULT_CAPACITY = 10;
        private const float THREASHOLD = 0.7f;


        public HashMap(Func<TKey, int> getHash, Func<TKey, TKey, bool> equals)
        {
            if (getHash == null) throw new ArgumentNullException("getHash");
            if (equals == null) throw new ArgumentNullException("equals");

            _getHash = getHash;
            _equal = equals;

            _capacity = DEFAULT_CAPACITY;
            _totalElements = 0;
            _emptyKey = default(TKey);
        }

        private float GetLoadFactor()
        {
            if (_totalElements == 0) return 0;
            return ((float)_capacity) / _totalElements;
        }

        private void Realloc()
        {
            var newCapacity = _capacity*2;
            if (newCapacity < _capacity) newCapacity = int.MaxValue;

            var newKeysStorage = new TKey[newCapacity];
            var newValuesStorage = new TValue[newCapacity];

            if (!EqualityComparer<TKey>.Default.Equals(default(TKey), _emptyKey))
            {
                newKeysStorage.Fill(_emptyKey);
            }

            int elementsMoved = 0;
            for (int i = 0; i < _capacity && elementsMoved < _totalElements; i++)
            {
                var currentKey = _keys[i];
                var currentValue = _values[i];

                if (EqualityComparer<TKey>.Default.Equals(currentKey, _emptyKey)) continue;

                var newItemIndexCandidate = _getHash(currentKey)%newCapacity;
                var newItemIndex = FindFirstEmptyKeyCell(newItemIndexCandidate, newKeysStorage);

                newKeysStorage[newItemIndex] = currentKey;
                newValuesStorage[newItemIndex] = currentValue;
                elementsMoved++;
            }

            _keys = newKeysStorage;
            _values = newValuesStorage;
            _capacity = newCapacity;
        }



        private int FindFirstEmptyKeyCell(int newItemIndexCandidate, TKey[] newKeysStorage)
        {
            var currentIndex = newItemIndexCandidate;
            for (; currentIndex < newKeysStorage.Length; currentIndex++)
            {
                var currentKey = newKeysStorage[currentIndex];
                if (EqualityComparer<TKey>.Default.Equals(currentKey, _emptyKey)) return currentIndex;
            }

            //not found. let's search from the top
            currentIndex = 0;
            for (; currentIndex < newItemIndexCandidate; currentIndex++)
            {
                var currentKey = newKeysStorage[currentIndex];
                if (EqualityComparer<TKey>.Default.Equals(currentKey, _emptyKey)) return currentIndex; 
            }

            throw new InvalidOperationException("Can't find empty cell for a key");
        }

        public void Add(TKey key, TValue value)
        {
            var loadFactor = GetLoadFactor();
            if (loadFactor >= THREASHOLD)
            {
                Realloc();
            }
            var keyIndexCandidate = _getHash(key)%_capacity;

        }

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
