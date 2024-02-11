namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

sealed class GroupsModel
{
    private GroupsModel(EquatableDictionary<String, GroupModel> map) => _map = map;
    private readonly EquatableDictionary<String, GroupModel> _map;

    public static GroupsModel Create(EquatableList<RepresentableTypeModel> representableTypes)
    {
        var map = new Dictionary<String, ImmutableHashSet<RepresentableTypeModel>.Builder>();

        foreach(var (group, representableType) in representableTypes.SelectMany(t => t.Groups.Select(g => (g, t))))
        {
            if(!map.TryGetValue(group, out var signatures))
            {
                signatures = ImmutableHashSet.CreateBuilder<RepresentableTypeModel>();
                map.Add(group, signatures);
            }

            _ = signatures.Add(representableType);
        }

        var resultMap = map
            .Select(kvp =>
            (
                name: kvp.Key,
                model: new GroupModel(kvp.Key, kvp.Value.ToImmutable().AsEquatable()
            )))
            .ToDictionary(t => t.name, t => t.model)
            .AsEquatable();

        var result = new GroupsModel(resultMap);

        return result;
    }

    public GroupModel this[String name] => _map[name];
    public GroupModel this[GroupModel group] => this[group.Name];
    public IEnumerable<GroupModel> Groups => _map.Values;
    public IEnumerable<String> Names => _map.Keys;

    public override Boolean Equals(Object? obj) =>
        ReferenceEquals(obj, this)
        || obj is GroupsModel model
        && EqualityComparer<EquatableDictionary<String, GroupModel>>.Default.Equals(_map, model._map);
    public override Int32 GetHashCode() =>
        -2013957080 + EqualityComparer<EquatableDictionary<String, GroupModel>>.Default.GetHashCode(_map);
}
