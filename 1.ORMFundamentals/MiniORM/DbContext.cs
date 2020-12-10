namespace MiniORM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;

    public abstract class DbContext
    {
        private readonly DatabaseConnection connection;
        private readonly Dictionary<Type, PropertyInfo> dbSetProperties;

        protected DbContext(string connectionString)
        {
            this.connection = new DatabaseConnection(connectionString);
            this.dbSetProperties = this.DiscoverDbSets();

            using (new ConnectionManager(connection))
            {
                this.InitializeDbSets();
            }

            this.MapAllRelations();
        }

        private IEnumerable<TEntity> LoadTableEntities<TEntity>()
            where TEntity : class
        {
            Type table = typeof(TEntity);

            string[] columns = GetEntityColumnNames(table);

            string tableName = GetTableName(table);

            TEntity[] fetchedRows = this.connection.FetchResultSet<TEntity>(tableName, columns).ToArray();

            return fetchedRows;
        }

        private string[] GetEntityColumnNames(Type table)
        {
            string tableName = this.GetTableName(table);
            IEnumerable<string> dbColumns = this.connection.FetchColumnNames(tableName);

            string[] columns = table
                .GetProperties()
                .Where(x => dbColumns.Contains(x.Name) &&
                            !x.HasAttribute<NotMappedAttribute>() &&
                            AllowedSqlTypes.Contains(x.PropertyType))
                .Select(x => x.Name)
                .ToArray();

            return columns;
        }

        private void MapAllRelations()
        {
            foreach (var property in this.dbSetProperties)
            {
                Type dbSetType = property.Key;

                MethodInfo mapRelationsGeneric = typeof(DbContext)
                    .GetMethod("MapRelations", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(dbSetType);

                object dbSet = property.Value.GetValue(this);

                mapRelationsGeneric.Invoke(this, new[] { dbSet });
            }
        }

        private void MapCollection<TDbSet, TCollection>(DbSet<TDbSet> dbSet, PropertyInfo collectionProperty)
            where TDbSet : class, new() where TCollection : class, new()
        {
            Type entityType = typeof(TDbSet);
            Type collectionType = typeof(TCollection);

            PropertyInfo[] primaryKeys = collectionType
                .GetProperties()
                .Where(x => x.HasAttribute<KeyAttribute>())
                .ToArray();

            PropertyInfo primaryKey = primaryKeys.First();

            PropertyInfo foreignKey = entityType
                .GetProperties()
                .First(x => x.HasAttribute<KeyAttribute>());

            bool isManyToMany = primaryKeys.Length >= 2;

            if (isManyToMany)
            {
                primaryKey = collectionType.GetProperties()
                    .First(x => collectionType
                            .GetProperty(x.GetCustomAttribute<ForeignKeyAttribute>().Name)
                            .PropertyType == entityType);
            }

            DbSet<TCollection> navigationDbSet = (DbSet<TCollection>)this.dbSetProperties[collectionType].GetValue(this);

            foreach (TDbSet entity in dbSet)
            {
                object primaryKeyValue = foreignKey.GetValue(entity);

                TCollection[] navigationEntities = navigationDbSet
                    .Where(x => primaryKey.GetValue(x).Equals(primaryKeyValue))
                    .ToArray();

                ReflectionHelper.ReplaceBackingField(entity, collectionProperty.Name, navigationEntities);
            }
        }

        private void MapRelations<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            Type entityType = typeof(TEntity);

            MapNavigationProperties(dbSet);

            PropertyInfo[] collections = entityType
                .GetProperties()
                .Where(x => x.PropertyType.IsGenericType &&
                            x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                .ToArray();

            foreach (PropertyInfo collection in collections)
            {
                Type collectionType = collection.PropertyType.GenericTypeArguments.First();

                MethodInfo mapCollectionMethod = typeof(DbContext)
                    .GetMethod("MapCollection", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(entityType, collectionType);

                mapCollectionMethod.Invoke(this, new object[] { dbSet, collection });
            }
        }

        private void MapNavigationProperties<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            Type entityType = typeof(TEntity);

            PropertyInfo[] foreignKeys = entityType
                .GetProperties()
                .Where(x => x.HasAttribute<ForeignKeyAttribute>())
                .ToArray();

            foreach (PropertyInfo foreignKey in foreignKeys)
            {
                string navigationPropertyName = foreignKey
                    .GetCustomAttribute<ForeignKeyAttribute>().Name;

                PropertyInfo navigationProperty = entityType.GetProperty(navigationPropertyName);

                object navigationDbSet = this.dbSetProperties[navigationProperty.PropertyType]
                    .GetValue(this);

                PropertyInfo navigationPrimaryKey = navigationProperty.PropertyType.GetProperties()
                    .First(x => x.HasAttribute<KeyAttribute>());

                foreach (TEntity entity in dbSet)
                {
                    object foreignKeyValue = foreignKey.GetValue(entity);

                    object navigationPropertyValue =
                        ((IEnumerable<object>)navigationDbSet).First(x =>
                           navigationPrimaryKey.GetValue(x).Equals(foreignKeyValue));

                    navigationProperty.SetValue(entity, navigationPropertyValue);
                }
            }
        }

        private static bool IsObjectValid(object obj)
        {
            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> validationErrors = new List<ValidationResult>();

            bool validationResult =
                Validator.TryValidateObject(obj, validationContext, validationErrors, validateAllProperties: true);

            return validationResult;
        }

        private void PopulateDbSet<TEntity>(PropertyInfo dbSet)
            where TEntity : class, new()
        {
            IEnumerable<TEntity> entities = LoadTableEntities<TEntity>();
            DbSet<TEntity> dbSetInstance = new DbSet<TEntity>(entities);

            ReflectionHelper.ReplaceBackingField(this, dbSet.Name, dbSetInstance);
        }

        private void InitializeDbSets()
        {
            foreach (KeyValuePair<Type, PropertyInfo> property in this.dbSetProperties)
            {
                Type dbSetType = property.Key;
                PropertyInfo dbSetProperty = property.Value;

                MethodInfo populateDbSetGeneric = typeof(DbContext)
                    .GetMethod("PopulateDbSet", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(dbSetType);

                populateDbSetGeneric.Invoke(this, new[] { dbSetProperty });
            }
        }

        public void SaveChanges()
        {
            object[] dbSets = this.dbSetProperties
                .Select(x => x.Value.GetValue(this))
                .ToArray();

            foreach (IEnumerable<object> dbSet in dbSets)
            {
                object[] invalidEntities = dbSet.Where(x => !IsObjectValid(x)).ToArray();

                if (invalidEntities.Any())
                {
                    throw new InvalidOperationException($"{invalidEntities.Length} Invalid Entities found in {dbSet.GetType().Name}!");
                }
            }

            using (new ConnectionManager(connection))
            {
                using (SqlTransaction transaction = this.connection.StartTransaction())
                {
                    foreach (IEnumerable dbSet in dbSets)
                    {
                        Type dbSetType = dbSet.GetType().GetGenericArguments().First();

                        MethodInfo persistMethod = typeof(DbContext)
                            .GetMethod("Persist", BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(dbSetType);

                        try
                        {
                            persistMethod.Invoke(this, new object[] { dbSet });
                        }
                        catch (TargetInvocationException tie)
                        {
                            throw tie.InnerException;
                        }
                        catch (InvalidOperationException)
                        {
                            transaction.Rollback();
                            throw;
                        }
                        catch (SqlException)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private Dictionary<Type, PropertyInfo> DiscoverDbSets()
        {
            Dictionary<Type, PropertyInfo> dbSets = this.GetType().GetProperties()
                .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToDictionary(x => x.PropertyType.GetGenericArguments().First(), x => x);

            return dbSets;
        }

        private void Persist<TEntity>(DbSet<TEntity> dbSet)
         where TEntity : class, new()
        {
            string tableName = GetTableName(typeof(TEntity));
            string[] columns = this.connection.FetchColumnNames(tableName).ToArray();

            if (dbSet.ChangeTracker.Added.Any())
            {
                this.connection.InsertEntities(dbSet.ChangeTracker.Added, tableName, columns);
            }

            TEntity[] modifiedEntities = dbSet.ChangeTracker.GetModifiedEntities(dbSet).ToArray();

            if (modifiedEntities.Any())
            {
                this.connection.UpdateEntities(modifiedEntities, tableName, columns);
            }

            if (dbSet.ChangeTracker.Removed.Any())
            {
                this.connection.DeleteEntities(dbSet.ChangeTracker.Removed, tableName, columns);
            }
        }

        private string GetTableName(Type tableType)
        {
            string tableName = ((TableAttribute)Attribute.GetCustomAttribute(tableType, typeof(TableAttribute))).Name;

            if (tableName == null)
            {
                tableName = this.dbSetProperties[tableType].Name;
            }

            return tableName;
        }

        internal static readonly Type[] AllowedSqlTypes =
        {
            typeof(bool),
            typeof(string),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(decimal),
            typeof(double),
            typeof(DateTime)
        };
    }
}
