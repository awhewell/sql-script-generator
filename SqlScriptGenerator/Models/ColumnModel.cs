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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class ColumnModel : IEntity
    {
        public string Name { get; }

        public bool HasDefaultValue { get; }

        public bool IsCaseSensitive { get; }

        public bool IsIdentity { get; }

        public bool IsNullable { get; }

        public bool IsComputed { get; }

        public bool IsPrimaryKeyMember { get; }

        public string SqlType { get; set; }

        public ColumnCollection Parent { get; }

        IEntity IEntity.Parent => (IEntity)((ColumnCollection)Parent);

        public IEnumerable<IEntity> Children => new IEntity[0];

        public int Ordinal { get; private set; }

        public ColumnModel(
            ColumnCollection    parent,
            string              name,
            int                 ordinal,
            string              sqlType,
            bool                hasDefaultValue,
            bool                isCaseSensitive,
            bool                isComputed,
            bool                isIdentity,
            bool                isNullable,
            bool                isPrimaryKeyMember
        )
        {
            Parent =                parent;
            Name =                  name;
            Ordinal =               ordinal;
            SqlType =               sqlType;
            HasDefaultValue =       hasDefaultValue;
            IsCaseSensitive =       isCaseSensitive;
            IsComputed =            isComputed;
            IsIdentity =            isIdentity;
            IsNullable =            isNullable;
            IsPrimaryKeyMember =    isPrimaryKeyMember;
        }

        public override string ToString() => Name ?? "";
    }
}
