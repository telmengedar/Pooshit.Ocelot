# P00Sh:7 Ocelot Database Manager

Simple nonintrusive entitymanager to access a database without the need to include sql strings into your project.

## Compatibility

This library can interact with **Sqlite**, **PostgreSQL**, **MySQL**/**MariaDB** and **MSSql**

## Entities

To work with the entitymanager you need entities. This means a class with properties which acts as a model for your data. If you don't specify otherwise the entitymanager is automatically using your classname as tablename and the names of the properties as column names.

So a simple entity would look somehow like this

```
public class Entity {
	public long ID { get; set; }
	public string Name { get; set; }
}
```

There is no need for the property ID, but since most use cases have one it is included here as well.

## Attributes

There are several attributes to specify how your entity is created.

### Primary Key

To create a property as primary key you need to use the **PrimaryKey** attributes

```
public class Entity {
	[PrimaryKey]
	public long ID { get; set; }
	
	public string Name { get; set; }
}
```

### Index

To create an index for a property, use the attribute **Index**. You need to specify a name for the index. Properties which share the same name in their index are included in the same index.

```
public class Entity {
	[PrimaryKey]
	public long ID { get; set; }
	
	[Index("name")]
	public string Name { get; set; }
}
```

### Auto Increment

Properties which shall get an automatically increasing value when creating new entities have to use the attribute **AutoIncrement**

```
public class Entity {
	[PrimaryKey]
	[AutoIncrement]
	public long ID { get; set; }
	
	[Index("name")]
	public string Name { get; set; }
}
```

### Ignore

Properties not to be included in the database scheme for some reason are to be decorated with the **Ignore** attribute.

```
public class Entity {
	[PrimaryKey]
	[AutoIncrement]
	public long ID { get; set; }
	
	[Index("name")]
	public string Name { get; set; }
	
	[Ignore]
	public int SillyNumber { get; set; }
}
```

## Views

The entitymanager works with views like with entities. For creation you have to specify the code which is used to create the view using the **View** attribute . The attribute is used to specify an embedded resource which contains the sql code to create the view. After that you can use the entity manager to work with a view like an entity.

```
[View("Namespace.Resource.sql")]
public class SomeView {
	public string Name {get;set;}
	public int Number {get;set;}
}
```

## Table and Column mapping

If for some reason you don't want to use the name of the property for the columnname or the name of the class for the tablename, for instance you have to access an existing database, you can modify the mapping with attributes.

### Table name

The name of the table of an entity can be specified with the **Table** attribute.

```
[Table("Companies")]
public class LegacyEntity {
}
```

### Column name

The name of the column with represents a property can be specified with the **Column** attribute.

```
[Table("Companies")]
public class LegacyEntity {
	[Column("xa_kr11")]
	public int Number{get;set;}
}
```

## Entity Manager

The Entity Manager provides a fluent API to access your data in the database. This API is oriented at sql syntax.

### Connect to database

The entitymanager only connects to the database when an operation is executed. The connection details are specified when creating the entity manager.

```
EntityManager entitymanager = new EntityManager(DBClient.CreateSQLite("database.db3"));
```

### Creating entity schemes

To work with an entity you have to create the table for that entity at some point. This only works if the entity doesn't exist already.

```
entitymanager.Create<Entity>();
```

### Updating entity schemes

If your entity model changes at some point you can update the scheme in database. This requires all valuetypes to be specified with a default value. When the entity doesn't exist in database, this will create the table. This means you can also use this method to create entities.

```
entitymanager.UpdateScheme<Entity>();
```

### Inserting entities

To insert new entities into the database you have to use the insert method. You have to specify a value for all columns here which don't have a default value or are not automatically incremented.

```
entitymanager.Insert<Entity>().Columns(e=>e.Name).Values("Hans").Execute();
```

Normally this will return the count of the inserted rows but you can also return the id of the inserted row (if there is a valid id column)

```
entitymanager.Insert<Entity>().Columns(e=>e.Name).Values("Hans").ReturnID().Execute();
```

### Updating entities

```
entitymanager.Update<Entity>().Set(e=>e.Name=="Dieter").Where(e=>e.ID==1).Execute();
```

### Deleting entities

```
entitymanager.Delete<Entity>().Execute();
```

Since this will of course delete all of your data in the table a filter is advised.

```
entitymanager.Delete<Entity>().Where(e=>e.Name=="Lisa").Execute();
```

### Loading entities

To load entities from the database use the **LoadEntity** method.

```
entitymanager.LoadEntity<Entity>().Execute();
```

You can of course also specify a filter if you don't want to load all entities from the database.

```
entitymanager.Load<Entity>().Where(e=>e.ID>70 && e.ID<112).Execute();
```

### Loading values of entities

Sometimes you don't need the full entity but just values of an entity

```
entitymanager.Load<Entity>(e=>e.Name).ExecuteSet<string>();
```

**Load** is also used to load values like count or sum

```
entitymanager.Load<Entity>(DBFunction.Count).ExecuteScalar<long>();
```
