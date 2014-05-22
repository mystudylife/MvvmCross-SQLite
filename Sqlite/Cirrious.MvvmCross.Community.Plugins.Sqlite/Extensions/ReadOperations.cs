using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions.TextBlob;

using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using SQLiteConnection = Cirrious.MvvmCross.Community.Plugins.Sqlite.ISQLiteConnection;

namespace SQLiteNetExtensions.Extensions {
    public static class ReadOperations {
        public static T GetWithChildren<T>(this SQLiteConnection conn, object pk) where T : new() {
            var element = conn.Get<T>(pk);
            conn.GetChildren(ref element);
            return element;
        }

        /// <summary>
        ///		Executes the current <paramref name="query"/>
        ///		and enumerates it to a list.
        /// </summary>
        /// <remarks>
        ///		Warning. This method could cause serious performance
        ///		implications and should only be used for limited sets of data.
        /// </remarks>
        public static ICollection<T> WithChildren<T>(this ITableQuery<T> query, params string[] relationshipPropertyNames) where T : new() {
            List<T> results = query.ToList();

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (results.Count == 0) return results;

            query.Connection.SetChildren(results);

            return results;
        }

        public static void SetChildren<T>(this SQLiteConnection conn, IEnumerable<T> elements, params string[] relationshipPropertyNames) where T : new() {
            if (elements is ITableQuery<T>) {
                throw new InvalidOperationException("Use WithChildren<T> or enumerate the query before calling SetChildren<T>.");
            }

            var results = elements as ICollection<T> ?? elements.ToList();

            var relationshipProperties = typeof(T).GetRelationshipProperties().ToList();

            if (relationshipPropertyNames != null && relationshipPropertyNames.Any()) {
                foreach (var propertyName in relationshipPropertyNames) {
                    if (relationshipProperties.All(p => p.Name != propertyName)) {
                        throw new ArgumentException(String.Format("Invalid relationship property name '{0}'.", propertyName));
                    }
                }

                var toRemove = relationshipProperties.Where(p => !relationshipPropertyNames.Contains(p.Name)).ToList();

                foreach (var p in toRemove) {
                    relationshipProperties.Remove(p);
                }
            }

            foreach (var relationshipProperty in relationshipProperties) {
                var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

                if (relationshipAttribute is ManyToOneAttribute) {
                    conn.GetManyToOneChildren(results, relationshipProperty);
                }
                else if (relationshipAttribute is OneToManyAttribute) {
                    conn.GetOneToManyChildren(results, relationshipProperty);
                }
#if DEBUG
                else {
                    Debug.WriteLine("WARNING: RelationshipAttribute {0} is not yet supported by WithChildren or SetChildren.", relationshipAttribute.GetType().Name);
                }
#endif
            }
        }

        public static void GetChildren<T>(this SQLiteConnection conn, ref T element) where T : new() {
            foreach (var relationshipProperty in typeof(T).GetRelationshipProperties()) {
                conn.GetChild(ref element, relationshipProperty);
            }
        }

        public static void GetChild<T>(this SQLiteConnection conn, ref T element, string relationshipProperty) {
            conn.GetChild(ref element, typeof(T).GetProperty(relationshipProperty));
        }

        public static void GetChild<T>(this SQLiteConnection conn, ref T element, PropertyInfo relationshipProperty) {
            var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

            if (relationshipAttribute is OneToOneAttribute) {
                conn.GetOneToOneChild(ref element, relationshipProperty);
            }
            else if (relationshipAttribute is OneToManyAttribute) {
                conn.GetOneToManyChildren(ref element, relationshipProperty);
            }
            else if (relationshipAttribute is ManyToOneAttribute) {
                conn.GetManyToOneChild(ref element, relationshipProperty);
            }
            else if (relationshipAttribute is ManyToManyAttribute) {
                conn.GetManyToManyChildren(ref element, relationshipProperty);
            }
            else if (relationshipAttribute is TextBlobAttribute) {
                TextBlobOperations.GetTextBlobChild(ref element, relationshipProperty);
            }
        }

        private static void GetOneToOneChild<T>(this SQLiteConnection conn, ref T element,
                                                PropertyInfo relationshipProperty) {
            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            Debug.Assert(enclosedType == EnclosedType.None, "OneToOne relationship cannot be of type List or Array");

            var currentEntityPrimaryKeyProperty = type.GetPrimaryKey();
            var otherEntityPrimaryKeyProperty = entityType.GetPrimaryKey();
            Debug.Assert(currentEntityPrimaryKeyProperty != null || otherEntityPrimaryKeyProperty != null,
                         "At least one entity in a OneToOne relationship must have Primary Key");

            var currentEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty);
            var otherEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty, inverse: true);
            Debug.Assert(currentEntityForeignKeyProperty != null || otherEntityForeignKeyProperty != null,
                         "At least one entity in a OneToOne relationship must have Foreign Key");

            var hasForeignKey = otherEntityPrimaryKeyProperty != null && currentEntityForeignKeyProperty != null;
            var hasInverseForeignKey = currentEntityPrimaryKeyProperty != null && otherEntityForeignKeyProperty != null;
            Debug.Assert(hasForeignKey || hasInverseForeignKey,
                         "Missing either ForeignKey or PrimaryKey for a complete OneToOne relationship");

            var tableMapping = conn.GetMapping(entityType);
            Debug.Assert(tableMapping != null, "There's no mapping table for OneToOne relationship");

            var inverseProperty = type.GetInverseProperty(relationshipProperty);

            object value = null;
            if (hasForeignKey) {
                var foreignKeyValue = currentEntityForeignKeyProperty.GetValue(element, null);
                if (foreignKeyValue != null) {
                    value = conn.Find(foreignKeyValue, tableMapping);
                }
            }
            else {
                var primaryKeyValue = currentEntityPrimaryKeyProperty.GetValue(element, null);
                if (primaryKeyValue != null) {
                    var query = string.Format("select * from {0} where {1} = ? limit 1", entityType.Name,
                                              otherEntityForeignKeyProperty.Name);
                    value = conn.Query(tableMapping, query, primaryKeyValue).FirstOrDefault();
                    // Its a OneToOne, take only the first
                }
            }

            relationshipProperty.SetValue(element, value, null);

            if (value != null && inverseProperty != null) {
                inverseProperty.SetValue(value, element, null);
            }
        }


        private static void GetManyToOneChild<T>(this SQLiteConnection conn, ref T element,
                                                 PropertyInfo relationshipProperty) {
            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            Debug.Assert(enclosedType == EnclosedType.None, "ManyToOne relationship cannot be of type List or Array");

            var otherEntityPrimaryKeyProperty = entityType.GetPrimaryKey();
            Debug.Assert(otherEntityPrimaryKeyProperty != null,
                         "ManyToOne relationship destination must have Primary Key");

            var currentEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty);
            Debug.Assert(currentEntityForeignKeyProperty != null, "ManyToOne relationship origin must have Foreign Key");

            var tableMapping = conn.GetMapping(entityType);
            Debug.Assert(tableMapping != null, "There's no mapping table for OneToMany relationship destination");

            object value = null;
            var foreignKeyValue = currentEntityForeignKeyProperty.GetValue(element, null);
            if (foreignKeyValue != null) {
                value = conn.Find(foreignKeyValue, tableMapping);

                if (IsSoftDeleted(entityType, value)) {
                    value = null;
                }
            }

            relationshipProperty.SetValue(element, value, null);
        }

        private static void GetManyToOneChildren<T>(this SQLiteConnection conn, ICollection<T> elements, PropertyInfo relationshipProperty) where T : new() {

            if (!elements.Any()) {
                return;
            }

            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            Debug.Assert(enclosedType == EnclosedType.None, "ManyToOne relationship cannot be of type List or Array");

            var otherEntityPrimaryKeyProperty = entityType.GetPrimaryKey();
            Debug.Assert(otherEntityPrimaryKeyProperty != null,
                         "ManyToOne relationship destination must have Primary Key");

            var currentEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty);
            Debug.Assert(currentEntityForeignKeyProperty != null, "ManyToOne relationship origin must have Foreign Key");

            var tableMapping = conn.GetMapping(entityType);
            Debug.Assert(tableMapping != null, "There's no mapping table for OneToMany relationship destination");

            IEnumerable<object> foreignKeyValues = elements.Select(x => {
                var fkPropVal = currentEntityForeignKeyProperty.GetValue(x, null);

                return fkPropVal is Guid ? fkPropVal.ToString() : fkPropVal;
            }).Where(x => x != null).Distinct().ToList();

            var query = String.Format(
                "select * from \"{0}\" where \"{1}\" IN ({2})",
                tableMapping.TableName,
                otherEntityPrimaryKeyProperty.GetColumnName(),
                String.Join(",", foreignKeyValues.Select(x => "?"))
            );

            var softDeleteSql = GetSoftDeleteFilterSql(entityType);

            if (softDeleteSql != null) {
                query = String.Concat(query, " AND ", softDeleteSql);
            }

            var foreignValues = conn.Query(
                tableMapping,
                query,
                foreignKeyValues.ToArray()
            );

            foreach (var element in elements) {
                var foreignKeyValue = currentEntityForeignKeyProperty.GetValue(element, null);
                object value = null;

                if (foreignKeyValue != null) {
                    value = foreignValues.SingleOrDefault(x => otherEntityPrimaryKeyProperty.GetValue(x, null).Equals(foreignKeyValue));
                }

                relationshipProperty.SetValue(element, value, null);
            }
        }

        private static void GetOneToManyChildren<T>(this SQLiteConnection conn, ref T element,
                                                    PropertyInfo relationshipProperty) {
            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            Debug.Assert(enclosedType != EnclosedType.None, "OneToMany relationship must be a List or Array");

            var currentEntityPrimaryKeyProperty = type.GetPrimaryKey();
            Debug.Assert(currentEntityPrimaryKeyProperty != null, "OneToMany relationship origin must have Primary Key");

            var otherEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty, inverse: true);
            Debug.Assert(otherEntityForeignKeyProperty != null,
                         "OneToMany relationship destination must have Foreign Key to the origin class");
            var tableMapping = conn.GetMapping(entityType);
            Debug.Assert(tableMapping != null, "There's no mapping table for OneToMany relationship destination");

            var inverseProperty = type.GetInverseProperty(relationshipProperty);

            IEnumerable values = null;
            var primaryKeyValue = currentEntityPrimaryKeyProperty.GetValue(element, null);
            if (primaryKeyValue != null) {
                var query = string.Format("select * from {0} where {1} = ?", entityType.GetTableName(),
                                          otherEntityForeignKeyProperty.GetColumnName());

                var softDeleteSql = GetSoftDeleteFilterSql(entityType);

                if (softDeleteSql != null) {
                    query = String.Concat(query, " AND ", softDeleteSql);
                }

                var queryResults = conn.Query(tableMapping, query, primaryKeyValue is Guid ? primaryKeyValue.ToString() : primaryKeyValue);

                values = CreateEnclosedType(entityType, enclosedType, queryResults);
            }

            relationshipProperty.SetValue(element, values, null);

            if (inverseProperty != null && values != null) {
                // Establish inverse relationships (we already have that object anyway)
                foreach (var value in values) {
                    inverseProperty.SetValue(value, element, null);
                }
            }
        }

        private static void GetOneToManyChildren<T>(this SQLiteConnection conn, ICollection<T> elements, PropertyInfo relationshipProperty) {

            if (!elements.Any()) {
                return;
            }

            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            Debug.Assert(enclosedType != EnclosedType.None, "OneToMany relationship must be a List or Array");

            var currentEntityPrimaryKeyProperty = type.GetPrimaryKey();
            Debug.Assert(currentEntityPrimaryKeyProperty != null, "OneToMany relationship origin must have Primary Key");

            var otherEntityForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty, inverse: true);
            Debug.Assert(otherEntityForeignKeyProperty != null,
                         "OneToMany relationship destination must have Foreign Key to the origin class");

            var tableMapping = conn.GetMapping(entityType);
            Debug.Assert(tableMapping != null, "There's no mapping table for OneToMany relationship destination");

            var inverseProperty = type.GetInverseProperty(relationshipProperty);

            IEnumerable<object> primaryKeyValues = elements.Select(x => {
                var fkPropVal = currentEntityPrimaryKeyProperty.GetValue(x, null);

                return fkPropVal is Guid ? fkPropVal.ToString() : fkPropVal;
            }).Where(x => x != null).Distinct().ToList();

            var query = String.Format(
                "select * from \"{0}\" where \"{1}\" IN ({2})",
                entityType.GetTableName(),
                otherEntityForeignKeyProperty.GetColumnName(),
                String.Join(",", primaryKeyValues.Select(x => "?"))
            );

            var softDeleteSql = GetSoftDeleteFilterSql(entityType);

            if (softDeleteSql != null) {
                query = String.Concat(query, " AND ", softDeleteSql);
            }

            var children = conn.Query(
                tableMapping,
                query,
                primaryKeyValues.ToArray()
            );

            foreach (var element in elements) {
                var primaryKeyValue = currentEntityPrimaryKeyProperty.GetValue(element, null);

                var queryResults = children.Where(x => otherEntityForeignKeyProperty.GetValue(x, null).Equals(primaryKeyValue)).ToList();

                IEnumerable values = null;

                if (primaryKeyValue != null) {
                    values = CreateEnclosedType(entityType, enclosedType, queryResults);
                }

                relationshipProperty.SetValue(element, values, null);

                if (inverseProperty != null && values != null) {
                    // Establish inverse relationships (we already have that object anyway)
                    foreach (var value in values) {
                        inverseProperty.SetValue(value, element, null);
                    }
                }
            }
        }

        private static void GetManyToManyChildren<T>(this SQLiteConnection conn, ref T element,
                                                     PropertyInfo relationshipProperty) {
            var type = typeof(T);
            EnclosedType enclosedType;
            var entityType = relationshipProperty.GetEntityType(out enclosedType);

            var currentEntityPrimaryKeyProperty = type.GetPrimaryKey();
            var otherEntityPrimaryKeyProperty = entityType.GetPrimaryKey();
            var manyToManyMetaInfo = type.GetManyToManyMetaInfo(relationshipProperty);
            var currentEntityForeignKeyProperty = manyToManyMetaInfo.OriginProperty;
            var otherEntityForeignKeyProperty = manyToManyMetaInfo.DestinationProperty;
            var intermediateType = manyToManyMetaInfo.IntermediateType;
            var tableMapping = conn.GetMapping(entityType);

            Debug.Assert(enclosedType != EnclosedType.None, "ManyToMany relationship must be a List or Array");
            Debug.Assert(currentEntityPrimaryKeyProperty != null, "ManyToMany relationship origin must have Primary Key");
            Debug.Assert(otherEntityPrimaryKeyProperty != null, "ManyToMany relationship destination must have Primary Key");
            Debug.Assert(intermediateType != null, "ManyToMany relationship intermediate type cannot be null");
            Debug.Assert(currentEntityForeignKeyProperty != null, "ManyToMany relationship origin must have a foreign key defined in the intermediate type");
            Debug.Assert(otherEntityForeignKeyProperty != null, "ManyToMany relationship destination must have a foreign key defined in the intermediate type");
            Debug.Assert(tableMapping != null, "There's no mapping table defined for ManyToMany relationship origin");

            IEnumerable values = null;
            var primaryKeyValue = currentEntityPrimaryKeyProperty.GetValue(element, null);
            if (primaryKeyValue != null) {
                // Obtain the relationship keys
                var keysQuery = string.Format("select {0} from {1} where {2} = ?", otherEntityForeignKeyProperty.Name,
                                              intermediateType.Name, currentEntityForeignKeyProperty.Name);

                var query = string.Format("select * from {0} where {1} in ({2})", entityType.Name,
                                          otherEntityPrimaryKeyProperty.Name, keysQuery);

                var queryResults = conn.Query(tableMapping, query, primaryKeyValue);

                values = CreateEnclosedType(entityType, enclosedType, queryResults);
            }

            relationshipProperty.SetValue(element, values, null);

        }

        private static IEnumerable CreateEnclosedType(Type entityType, EnclosedType enclosedType, IList queryResults) {
            switch (enclosedType) {
                case EnclosedType.List:
                case EnclosedType.ObservableCollection:
                    var collectionType = enclosedType == EnclosedType.List ? typeof(List<>) : typeof(ObservableCollection<>);

                    var list = (IList)Activator.CreateInstance(collectionType.MakeGenericType(entityType));
                    foreach (var result in queryResults) {
                        list.Add(result);
                    }
                    return list;
                case EnclosedType.Array:
                    var array = Array.CreateInstance(entityType, queryResults.Count);
                    for (var i = 0; i < queryResults.Count; i++) {
                        array.SetValue(queryResults[i], i);
                    }
                    return array;
            }

            return null;
        }

        private static string GetSoftDeleteFilterSql(Type entityType) {
            var entitySoftDeleteColumn = entityType.GetSoftDeleteColumn();

            if (entitySoftDeleteColumn == null) return null;

            if (entitySoftDeleteColumn.PropertyType == typeof(DateTime?)) {
                return String.Format("{0} IS NULL", entitySoftDeleteColumn.GetColumnName());
            }
            if (entitySoftDeleteColumn.PropertyType == typeof(bool)) {
                return String.Format("{0} = 0", entitySoftDeleteColumn.GetColumnName());
            }

            Debug.Assert(true, "SoftDeleteColumn property type must be a nullable datetime or boolean.");

            return null;
        }

        private static bool IsSoftDeleted(Type entityType, object value) {
            var softDeleteColumn = entityType.GetSoftDeleteColumn();

            if (softDeleteColumn != null && value != null) {
                if (softDeleteColumn.PropertyType == typeof(DateTime?)) {
                    return ((DateTime?)softDeleteColumn.GetValue(value, null)).HasValue;
                }

                if (softDeleteColumn.PropertyType == typeof(bool)) {
                    return (bool)softDeleteColumn.GetValue(value, null);
                }
            }

            return false;
        }
    }
}
