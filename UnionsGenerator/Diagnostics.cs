namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

internal static partial class Diagnostics
{
    private const String _category = "RhoMicro.CodeAnalysis.UnionsGenerator";

    //IMPORTANT: append new values at the end to preserve error codes
    private enum Id
    {
        DuplicateUnionTypeAttributes = 1,
        GeneratorException = 2,
        MissingUnionTypeAttribute = 3,
        NonPartialDeclaration = 4,
        StaticTarget = 5,
        RecordTarget = 6,
        TooManyTypes = 7,
        ImplicitConversionOptionOnNonSolitary = 8,
        ImplicitConversionOptionOnSolitary = 9,
        InvalidAttributeTarget = 10,
        AliasCollision = 11,
        UnionTypeSettingsOnNonUnionType = 12,
        RepresentableTypeIsSupertype = 13,
        RepresentableTypeIsInterface = 14,
        ReservedGenericParameterName = 15,
        UnknownGenericParameterName = 16,
        PossibleBoxingStrategy = 17,
        BoxingStrategy = 18,
        PossibleTleStrategy = 19,
        TleStrategy = 20,
        SmallGenericUnion = 21,
        GenericViolationStrategy = 22,
        GenericRelation = 23,
        BidirectionalRelation = 24,
        DuplicateRelation = 25
    }
    public static Diagnostic DuplicateRelation(Location location, String relationName) =>
        Create(Id.DuplicateRelation, location, relationName);
    public static Diagnostic BidirectionalRelation(Location location, String relationName) =>
        Create(Id.BidirectionalRelation, location, relationName);
    public static Diagnostic GenericRelation(Location location) =>
        Create(Id.GenericRelation, location);
    public static Diagnostic GenericViolationStrategy(Location location, String typeName) =>
        Create(Id.GenericViolationStrategy, location, typeName);
    public static Diagnostic SmallGenericUnion(Location location) =>
        Create(Id.SmallGenericUnion, location);
    public static Diagnostic PossibleBoxingStrategy(Location location, String typeName) =>
        Create(Id.PossibleBoxingStrategy, location, typeName);
    public static Diagnostic BoxingStrategy(Location location, String typeName) =>
        Create(Id.BoxingStrategy, location, typeName);
    public static Diagnostic PossibleTleStrategy(Location location, String typeName) =>
        Create(Id.PossibleTleStrategy, location, typeName);
    public static Diagnostic TleStrategy(Location location, String typeName) =>
        Create(Id.TleStrategy, location, typeName);
    public static Diagnostic UnknownGenericParameterName(Location location, String name) =>
        Create(Id.UnknownGenericParameterName, location, name);
    public static Diagnostic ReservedGenericParameterName(Location location, String name) =>
        Create(Id.ReservedGenericParameterName, location, name);
    public static Diagnostic RepresentableTypeIsInterface(Location location, String representableTypeName) =>
        Create(Id.RepresentableTypeIsInterface, location, representableTypeName);
    public static Diagnostic RepresentableTypeIsSupertype(Location location, String representableTypeName) =>
        Create(Id.RepresentableTypeIsSupertype, location, representableTypeName);
    public static Diagnostic UnionTypeSettingsOnNonUnionType(Location location) =>
        Create(Id.UnionTypeSettingsOnNonUnionType, location);
    public static Diagnostic AliasCollision(Location location, String representableTypeName) =>
        Create(Id.AliasCollision, location, representableTypeName);
    public static Diagnostic InvalidAttributeTarget(Location location) =>
        Create(Id.InvalidAttributeTarget, location);
    public static Diagnostic TooManyTypes(Location location) =>
        Create(Id.TooManyTypes, location);
    public static Diagnostic StaticTarget(Location location) =>
        Create(Id.StaticTarget, location);
    public static Diagnostic RecordTarget(Location location) =>
        Create(Id.RecordTarget, location);
    public static Diagnostic NonPartialDeclaration(Location location) =>
        Create(Id.NonPartialDeclaration, location);
    public static Diagnostic MissingUnionTypeAttribute(Location location) =>
        Create(Id.MissingUnionTypeAttribute, location);
    public static Diagnostic GeneratorException(Exception exception) =>
        Create(Id.GeneratorException, Location.None, exception.Message);
    public static Diagnostic DuplicateUnionTypeAttributes(String unionTypeName, Location location) =>
        Create(Id.DuplicateUnionTypeAttributes, location, unionTypeName);
    public static Diagnostic ImplicitConversionOptionOnNonSolitary(Location location) =>
        Create(Id.ImplicitConversionOptionOnNonSolitary, location);
    public static Diagnostic ImplicitConversionOptionOnSolitary(String unionTypeName, String representableTypeName, Location location) =>
        Create(Id.ImplicitConversionOptionOnSolitary, location, $"ISuperset<{representableTypeName}, {unionTypeName}>");
}
