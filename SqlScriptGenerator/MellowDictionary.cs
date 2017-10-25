// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    /// <summary>
    /// A dictionary that does not throw exceptions when you try to read or write keys that do not exist.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MellowDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _Dictionary;

        public TValue this[TKey key]
        {
            get {
                _Dictionary.TryGetValue(key, out TValue result);
                return result;
            }
            set {
                if(_Dictionary.ContainsKey(key)) {
                    _Dictionary[key] = value;
                } else {
                    _Dictionary.Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys => _Dictionary.Keys;

        public ICollection<TValue> Values => _Dictionary.Values;

        public int Count => _Dictionary.Count;

        public bool IsReadOnly => false;

        public MellowDictionary() => _Dictionary = new Dictionary<TKey, TValue>();

        public MellowDictionary(IEqualityComparer<TKey> comparer) => _Dictionary = new Dictionary<TKey, TValue>(comparer);

        public void Add(TKey key, TValue value) => _Dictionary.Add(key, value);

        public void Add(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public void Clear() => _Dictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => _Dictionary.Contains(item);

        public bool ContainsKey(TKey key) => _Dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _Dictionary.GetEnumerator();

        public bool Remove(TKey key) => _Dictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public bool TryGetValue(TKey key, out TValue value) => _Dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _Dictionary.GetEnumerator();
    }
}
