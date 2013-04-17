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
        private readonly TValue _emptyValue;

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
            if (newCapacity < _capacity) newCapacity = int.MaxValue;

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
            return EqualityComparer<TKey>.Default.Equals(_keys[cellIndex], _emptyKey);
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

            var maybeAnItemIndex = _keys.FindItemCycled(itemIndexCandidate, aKey => IsEmptyKey(aKey) || _equal(aKey, key));
            if (!maybeAnItemIndex.HasValue) throw new InvalidOperationException("Internal error: no free space");

            var anItemIndex = maybeAnItemIndex.Value;

            //ok, what we actually have here?
            if (_equal(_keys[anItemIndex], key))
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
            return _keys.FindItemCycled(candidateIndex, aKey => _equal(aKey, key)).HasValue;
        }

        public ICollection<TKey> Keys
        {
            get { return _keys.Where(key => !IsEmptyKey(key)).ToArray(); }
        }

        public bool Remove(TKey key)
        {
            var itemIndexCandidate = _getHash(key)%_capacity;
            var maybeItemIndex = _keys.FindItemCycled(itemIndexCandidate, aKey => _equal(aKey, key) || IsEmptyKey(aKey));
            if (!maybeItemIndex.HasValue) throw new InvalidOperationException("some shit happened"); //there is must be at least one empty cell or cell with key provided.
            var itemIndex = maybeItemIndex.Value;
            if (IsEmptyKey(_keys[itemIndex])) return false;

            _keys[itemIndex] = _emptyKey;
            _values[itemIndex] = _emptyValue;
            _totalElements--;

            //depending on remove policy, we could shrink our keys and values storages
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values
        {
            get { return _values.Where(val => !IsEmptyValue(val)).ToArray(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
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
