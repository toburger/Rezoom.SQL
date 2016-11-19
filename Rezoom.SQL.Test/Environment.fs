﻿[<AutoOpen>]
module Rezoom.SQL.Test.Environment
open NUnit.Framework
open FsUnit
open System
open System.Reflection
open System.IO
open System.Collections.Generic
open Rezoom.SQL

let userModelByName name =
    let assemblyFolder = Path.GetDirectoryName(Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
    let resolutionFolder = Path.Combine(assemblyFolder, "../../" + name)
    UserModel.Load(resolutionFolder, ".")

let userModel1() = userModelByName "user-model-1"

let userModel2() = userModelByName "user-model-2"

let expectError (msg : string) (sql : string) =
    let userModel = userModel1()
    try
        ignore <| CommandEffect.OfSQL(userModel.Model, "anonymous", sql)
        failwith "Should've thrown an exception!"
    with
    | :? SourceException as exn ->
        printfn "\"%s\"" exn.Message
        Assert.AreEqual(msg, exn.Reason.Trim())

let dispenserParameterIndexer() =
    let dict = Dictionary()
    let mutable last = -1
    { new IParameterIndexer with
        member __.ParameterIndex(par) =
            let succ, value = dict.TryGetValue(par)
            if succ then value
            else
                last <- last + 1
                dict.[par] <- last
                last
    }

