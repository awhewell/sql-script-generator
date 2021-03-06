﻿// Copyright © 2017 onwards, Andrew Whewell
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class TemplateModel
    {
        public DatabaseModel Database { get; set; }

        public SchemaModel Schema { get; set; }

        public IEntity Entity { get; set; }

        public List<ColumnModel> Columns { get; } = new List<ColumnModel>();

        public MellowDictionary<string, ColumnModel> ColumnsByName { get; } = new MellowDictionary<string, ColumnModel>();

        public MellowDictionary<int, ColumnModel> ColumnsByOrdinal { get; } = new MellowDictionary<int, ColumnModel>();

        public bool IsCaseSensitive { get; }

        public TableModel Table { get; set; }

        public ViewModel View { get; set; }

        public UserDefinedTableTypeModel UDTT { get; set; }

        public TemplateModel(bool isCaseSensitive)
        {
            IsCaseSensitive = isCaseSensitive;
            if(!IsCaseSensitive) {
                ColumnsByName = new MellowDictionary<string, ColumnModel>();
            }
        }

        public string ApplyModelToFileSpec(string fileSpec)
        {
            string result = null;

            if(fileSpec != null) {
                var regex = new Regex(@"(\{(?<key>database|schema|entity|table|view|udtt)\})", RegexOptions.IgnoreCase);
                var buffer = new StringBuilder(fileSpec);
                foreach(var match in regex.Matches(fileSpec).OfType<Match>().OrderByDescending(r => r.Index)) {
                    if(match.Success) {
                        var replaceWith = "";
                        switch((match.Groups["key"].Value ?? "").ToLower()) {
                            case "database":    replaceWith = Database?.Name ?? ""; break;
                            case "schema":      replaceWith = Schema?.Name ?? ""; break;
                            case "entity":      replaceWith = Entity?.Name ?? ""; break;
                            case "table":       replaceWith = Table?.Name ?? ""; break;
                            case "view":        replaceWith = View?.Name ?? ""; break;
                            case "udtt":        replaceWith = UDTT?.Name ?? ""; break;
                        }

                        buffer.Remove(match.Index, match.Length);
                        buffer.Insert(match.Index, replaceWith);
                    }
                }

                result = buffer.ToString();
            }

            return result;
        }
    }
}
