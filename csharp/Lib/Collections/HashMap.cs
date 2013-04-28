using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lib.Extensions;

namespace Lib.Collections
{
    public class HashMap<TKey,TValue> : IDictionary<TKey,TValue>
    {
        private readonly Func<TKey, int> _getHash;
        private readonly Func<TKey, TKey, bool> _keysEqual;
        private readonly Func<TValue, TValue, bool> _valuesEqual; 

        private readonly TKey _emptyKey;
        private readonly TValue _emptyValue;

        private TKey[] _keys;
        private TValue[] _values;
        private int _totalElements;
        private int _capacity;

        private const int DEFAULT_CAPACITY = 10;
        private const float THREASHOLD = 0.7f;


        public HashMap(Func<TKey, int> getHash, Func<TKey, TKey, bool> keysEquals, Func<TValue, TValue, bool> valuesEqual)
        {
            if (getHash == null) throw new ArgumentNullException("getHash");
            if (keysEquals == null) throw new ArgumentNullException("keysEquals");
            if (valuesEqual == null) throw new ArgumentNullException("valuesEqual");

            _getHash = getHash;
            _keysEqual = keysEquals;
            _valuesEqual = valuesEqual;

            _capacity = DEFAULT_CAPACITY;
            _totalElements = 0;

            _emptyKey = default(TKey);
            _emptyValue = default(TValue);
        }

        private float GetLoadFactor()
        {
            if (_totalElements == 0) return 0;
            return ((float)_capacity) / _totalElements;
        }

        private void Realloc()
        {
            var newCapacity = _capacity*2;
            if (newCapacity < _capacity) newCapacity = int.MaxValue; //in case of integer overflow

            var newKeysStorage = new TKey[newCapacity];
            var newValuesStorage = new TValue[newCapacity];

            if (!EqualityComparer<TKey>.Default.Equals(default(TKey), _emptyKey))
            {
                newKeysStorage.Fill(_emptyKey);
            }

            if (!EqualityComparer<TValue>.Default.Equals(default(TValue), _emptyValue))
            {
                newValuesStorage.Fill(_emptyValue);
            }

            var elementsMoved = 0;
            for (var i = 0; i < _capacity && elementsMoved < _totalElements; i++)
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

        private bool IsKeyCellEmpty(int cellIndex)
        {
            return IsEmptyKey(_keys[cellIndex]);
        }

        private bool IsEmptyValue(TValue val)
        {
            return EqualityComparer<TValue>.Default.Equals(val, _emptyValue);
        }

        private bool IsEmptyKey(TKey key)
        {
            return EqualityComparer<TKey>.Default.Equals(key, _emptyKey);
        }

        private int FindFirstEmptyKeyCell(int newItemIndexCandidate, TKey[] newKeysStorage)
        {
            var maybeResult = newKeysStorage.FindItemCycled(newItemIndexCandidate, IsEmptyKey);
            if (maybeResult.HasValue) return maybeResult.Value;

            throw new InvalidOperationException("Can't find empty cell for a key");
        }

        public void Add(TKey key, TValue value)
        {
            AddOrReplace(key, value, DuplicateKeyResolutionPolicy.Throw);
        }

        public enum DuplicateKeyResolutionPolicy
        {
            Throw,
            DoNothing,
            ReplaceOnlyValue,
            ReplaceKeyAndValue
        }

        public HashMap<TKey, TValue> AddOrReplace(TKey key, TValue value, DuplicateKeyResolutionPolicy policy)
        {
            var loadFactor = GetLoadFactor();
            if (loadFactor >= THREASHOLD)
            {
                Realloc();
            }
            var itemIndexCandidate = _getHash(key) % _capacity;

            var maybeAnItemIndex = _keys.FindItemCycled(itemIndexCandidate, aKey => IsEmptyKey(aKey) || _keysEqual(aKey, key));
            if (!maybeAnItemIndex.HasValue) throw new InvalidOperationException("Internal error: no free space");

            var anItemIndex = maybeAnItemIndex.Value;

            //ok, what we actually have here?
            if (_keysEqual(_keys[anItemIndex], key))
            {
                var alreadyExistingKeyIndex = anItemIndex;
                switch (policy)
                {
                    case DuplicateKeyResolutionPolicy.Throw:
                        throw new ArgumentException("An element with the same key already exists");

                    case DuplicateKeyResolutionPolicy.DoNothing:
                        break;

                    case DuplicateKeyResolutionPolicy.ReplaceOnlyValue:
                        _values[alreadyExistingKeyIndex] = value;
                        break;

                    case DuplicateKeyResolutionPolicy.ReplaceKeyAndValue:
                        _keys[anItemIndex] = key;
                        _values[anItemIndex] = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("policy");
                }

            }
            else
            {
                //by exclusion, we have an empty cell index
                var emptyCellIndex = anItemIndex;

                _keys[emptyCellIndex] = key;
                _values[emptyCellIndex] = value;
                _totalElements++;
            }

            return this;
        } 

        public bool ContainsKey(TKey key)
        {
            var candidateIndex = _getHash(key) % _capacity;
            return _keys.FindItemCycled(candidateIndex, aKey => _keysEqual(aKey, key)).HasValue;
        }

        public ICollection<TKey> Keys
        {
            get { return _keys.Where(key => !IsEmptyKey(key)).ToArray(); }
        }

        private int? GetItemOrFirstEmptyCellIndex(TKey key)
        {
            var itemIndexCandidate = _getHash(key) % _capacity;
            return _keys.FindItemCycled(itemIndexCandidate, aKey => _keysEqual(aKey, key) || IsEmptyKey(aKey));

        }

        private int? GetItemIndex(TKey key)
        {
            var maybeItemIndex = GetItemOrFirstEmptyCellIndex(key);
            if (!maybeItemIndex.HasValue) throw new InvalidOperationException("some shit happened"); //there is must be at least one empty cell or cell with key provided.
            var itemIndex = maybeItemIndex.Value;
            if (IsKeyCellEmpty(itemIndex)) return null;
            return itemIndex;
        }

        public bool Remove(TKey key)
        {
            var maybeItemIndex = GetItemIndex(key);
            if (!maybeItemIndex.HasValue) return false;
            
            var itemIndex = maybeItemIndex.Value;
            _keys[itemIndex] = _emptyKey;
            _values[itemIndex] = _emptyValue;
            _totalElements--;

            //depending on remove policy, we could shrink our keys and values storages
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

            var maybeItemIndex = GetItemIndex(key);
            if (!maybeItemIndex.HasValue) return false;

            var itemIndex = maybeItemIndex.Value;
            value = _values[itemIndex];

            return true;
        }

        public ICollection<TValue> Values
        {
            get { return _values.Where(val => !IsEmptyValue(val)).ToArray(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (TryGetValue(key, out result)) return result;
                throw new KeyNotFoundException();
            }
            set
            {
                AddOrReplace(key, value, DuplicateKeyResolutionPolicy.ReplaceOnlyValue);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _keys.Fill(_emptyKey);
            _values.Fill(_emptyValue);
            _totalElements = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            return TryGetValue(item.Key, out value) && _valuesEqual(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var contents = this.ToArray();
            Array.Copy(contents, 0, array, arrayIndex, contents.Length);
        }

        public int Count
        {
            get { return _totalElements; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var maybeItemIndex = GetItemIndex(item.Key);
            if (!maybeItemIndex.HasValue) return false;
            
            var itemIndex = maybeItemIndex.Value;
            if (_valuesEqual(item.Value, _values[itemIndex]))
            {
                _totalElements--;
                _keys[itemIndex] = _emptyKey;
                _values[itemIndex] = _emptyValue;
                return true;
            }
            return false;
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>
        {
            private HashMap<TKey, TValue> _container;
            private int _currentIndex = -1;
            private int _valuesEnumerated = 0;

            public Enumerator(HashMap<TKey, TValue> container)
            {
                _container = container;
            }

            public void Dispose()
            {
                _container = null;
                _currentIndex = -1;
                _valuesEnumerated = -1;
            }

            public bool MoveNext()
            {
                if (_valuesEnumerated == _container._totalElements) return false;
                while (_currentIndex < _container._capacity)
                {
                    _currentIndex++; //to accomodate to -1
                    if (_container.IsKeyCellEmpty(_currentIndex)) continue;

                    _valuesEnumerated++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _currentIndex = -1;
                _valuesEnumerated = 0;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(_container._keys[_currentIndex],
                                                          _container._values[_currentIndex]);
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
