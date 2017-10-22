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

namespace SqlScriptGenerator.SqlServer.Models
{
    class IS_Column
    {
        public string   TABLE_CATALOG               { get; set; }
        public string   TABLE_SCHEMA                { get; set; }
        public string   TABLE_NAME                  { get; set; }
        public string   COLUMN_NAME                 { get; set; }
        public int      ORDINAL_POSITION            { get; set; }
        public string   COLUMN_DEFAULT              { get; set; }
        public string   IS_NULLABLE                 { get; set; }
        public string   DATA_TYPE                   { get; set; }
        public long?    CHARACTER_MAXIMUM_LENGTH    { get; set; }
        public long?    CHARACTER_OCTET_LENGTH      { get; set; }
        public int?     NUMERIC_PRECISION           { get; set; }
        public int?     NUMERIC_PRECISION_RADIX     { get; set; }
        public int?     NUMERIC_SCALE               { get; set; }
        public int?     DATETIME_PRECISION          { get; set; }
        public string   CHARACTER_SET_CATALOG       { get; set; }
        public string   CHARACTER_SET_SCHEMA        { get; set; }
        public string   COLLATION_CATALOG           { get; set; }
        public string   COLLATION_SCHEMA            { get; set; }
        public string   COLLATION_NAME              { get; set; }
        public string   DOMAIN_CATALOG              { get; set; }
        public string   DOMAIN_SCHEMA               { get; set; }
        public string   DOMAIN_NAME                 { get; set; }

        public bool     IsNullable      { get { return IS_NULLABLE != "NO"; } }
        public bool?    IsCaseSensitive { get { return COLLATION_NAME?.Contains("_CI_"); } }
    }
}
