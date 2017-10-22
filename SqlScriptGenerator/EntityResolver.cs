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
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    static class EntityResolver
    {
        public static string[] SplitEntityPathParts(string entityPath)
        {
            var result = new List<string>();
            var cleanEntityPath = (entityPath ?? "").Trim();

            if(cleanEntityPath.Length > 0) {
                var bracketNestLevel = 0;
                var currentPart = new StringBuilder();
                for(var i = 0;i < cleanEntityPath.Length;++i) {
                    var ch = cleanEntityPath[i];
                    switch(ch) {
                        case '[':
                            if(bracketNestLevel++ > 0) {
                                goto default;
                            }
                            break;
                        case ']':
                            if(--bracketNestLevel > 0) {
                                goto default;
                            }
                            break;
                        case '.':
                            if(bracketNestLevel > 0) {
                                goto default;
                            } else {
                                result.Add(currentPart.ToString());
                                currentPart.Clear();
                            }
                            break;
                        default:
                            currentPart.Append(ch);
                            break;
                    }
                }

                if(bracketNestLevel > 0) {
                    result.Clear();
                } else {
                    result.Add(currentPart.ToString());
                }
            }


            return result.ToArray();
        }

        public static IEntity Resolve(DatabaseModel database, string[] entityPathParts)
        {
            IEntity result = null;

            var idx = 0;
            switch(entityPathParts.Length) {
                case 1:
                    result = InvertModel(database).FirstOrDefault(r => EntityMatchesName(r, entityPathParts[idx]));
                    break;
                case 2:
                    var schema = database.Schemas.Values.FirstOrDefault(r => EntityMatchesName(r, entityPathParts[idx]));
                    result = schema?.Children.FirstOrDefault(r => EntityMatchesName(r, entityPathParts[idx + 1]));
                    break;
                case 3:
                    if(EntityMatchesName(database, entityPathParts[idx])) {
                        ++idx;
                        goto case 2;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        private static IEntity[] InvertModel(DatabaseModel database)
        {
            var result =
                database
                .Schemas.Values.SelectMany(r => r.Children.OrderBy(i => i.Name))
                .Concat(database.Schemas.Values.OrderBy(r => r.Name))
                .Concat(new IEntity[] { database})
                .ToArray();

            return result;
        }

        public static bool EntityMatchesName(IEntity entity, string name)
        {
            return String.Equals(entity.Name, name, entity.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        public static IEnumerable<IEntity> EntityParents(IEntity entity)
        {
            var result = new List<IEntity>();

            for(var parent = entity?.Parent;parent != null;parent = parent.Parent) {
                result.Add(parent);
            }

            return result;
        }

        public static IEnumerable<IEntity> EntityDescendents(IEntity entity)
        {
            var result = new List<IEntity>();
            if(entity != null) {
                recurse(result, entity);
            }

            void recurse(List<IEntity> list, IEntity innerEntity)
            {
                foreach(var child in innerEntity.Children) {
                    list.Add(child);
                    recurse(list, child);
                }
            }

            return result;
        }

        public static SchemaModel FindSchema(IEntity entity)
        {
            SchemaModel result = null;

            if(entity != null) {
                result = entity as SchemaModel;
                if(result == null) {
                    foreach(var parent in EntityParents(entity)) {
                        result = parent as SchemaModel;
                        if(result != null) {
                            break;
                        }
                    }
                }

                if(result == null) {
                    foreach(var descendent in EntityDescendents(entity)) {
                        result = descendent as SchemaModel;
                        if(result != null) {
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }
}
