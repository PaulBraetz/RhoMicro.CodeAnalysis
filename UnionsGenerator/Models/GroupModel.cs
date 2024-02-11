namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;

sealed record GroupModel(String Name, EquatableSet<RepresentableTypeModel> Members);
