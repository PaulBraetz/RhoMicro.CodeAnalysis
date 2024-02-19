namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

sealed record GroupModel(String Name, EquatableSet<RepresentableTypeModel> Members) : IModel<GroupModel>
{
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<GroupModel>
        => visitor.Visit(this);
}
