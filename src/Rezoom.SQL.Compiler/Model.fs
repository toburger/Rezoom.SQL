﻿namespace Rezoom.SQL.Compiler
open System.Collections.Generic

type DatabaseBuiltin =
    {   Functions : Map<Name, FunctionType>
    }

type QualifiedObjectName =
    {   SchemaName : Name
        ObjectName : Name
    }
    override this.ToString() =
        this.SchemaName.Value + "." + this.ObjectName.Value

type Model =
    {   Schemas : Map<Name, Schema>
        DefaultSchema : Name
        TemporarySchema : Name
        Builtin : DatabaseBuiltin
    }
    member this.Schema(name : Name option) =
        this.Schemas |> Map.tryFind (name |? this.DefaultSchema)

and Schema =
    {   SchemaName : Name
        Objects : Map<Name, SchemaObject>
    }
    static member Empty(name) =
        {   SchemaName = name
            Objects = Map.empty
        }
    member this.ContainsObject(name : Name) = this.Objects.ContainsKey(name)

and SchemaObject =
    | SchemaTable of SchemaTable
    | SchemaView of SchemaView
    | SchemaIndex of SchemaIndex

and SchemaIndex =
    {   TableName : QualifiedObjectName
        IndexName : Name
        Columns : Name Set
    }

and SchemaForeignKey =
    {   ToTable : QualifiedObjectName
        ToColumns : Name Set
        OnDelete : OnDeleteAction option
    }

and SchemaConstraintType =
    | PrimaryKeyConstraintType of auto : bool
    | ForeignKeyConstraintType of SchemaForeignKey
    | CheckConstraintType
    | UniqueConstraintType

and SchemaConstraint =
    {   ConstraintType : SchemaConstraintType
        TableName : QualifiedObjectName
        ConstraintName : Name
        /// Which columns this constraint relates to in the table.
        Columns : Name Set
    }

and SchemaReverseForeignKey =
    {   FromTable : QualifiedObjectName
        FromConstraint : Name
        OnDelete : OnDeleteAction option
    }

and SchemaTable =
    {   Name : QualifiedObjectName
        Columns : Map<Name, SchemaColumn>
        Indexes : Map<Name, SchemaIndex>
        Constraints : Map<Name, SchemaConstraint>
        ReverseForeignKeys : SchemaReverseForeignKey Set
    }

and SchemaColumn =
    {   TableName : QualifiedObjectName
        ColumnName : Name
        /// True if this column is part of the table's primary key.
        PrimaryKey : bool
        DefaultValue : Expr option
        ColumnType : ColumnType
        ColumnTypeName : TypeName
        Collation : Name option
    }

and SchemaView =
    {   SchemaName : Name
        ViewName : Name
        CreateDefinition : CreateViewStmt
    }
    member this.Definition = this.CreateDefinition.AsSelect

