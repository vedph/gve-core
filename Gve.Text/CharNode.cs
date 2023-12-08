using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// A single character in a string of nodes, where each character node points
/// to its left sibling.
/// </summary>
public class CharNode
{
    private char _value;
    private List<char>? _prevValues;
    private Dictionary<string, object>? _coords;

    /// <summary>
    /// Gets or sets the identifier. This is an arbitrary numeric ID which is
    /// granted to be unique in the string of nodes.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the ordinal number of the character in the original string.
    /// Newly added characters have always 0 as their ordinals. Ordinals are
    /// used by operations working on nodes, which always refer to the original
    /// string only.
    /// </summary>
    public int Ordinal { get; }

    /// <summary>
    /// Gets or sets the character value.
    /// </summary>
    public char Value
    {
        get => _value;
        set
        {
            if (_value != value && _value != default)
            {
                _prevValues ??= [];
                _prevValues.Insert(0, _value);
            }
            _value = value;
        }
    }

    /// <summary>
    /// Gets all the previous values set for this character. The most recent
    /// values are at the top. If the character never changed, this is empty.
    /// </summary>
    public IList<char> PreviousValues
    {
        get
        {
            return (IList<char>?)_prevValues ?? Array.Empty<char>();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this node was deleted. A deleted
    /// node remains in the set, and no changes are made to the left node
    /// pointers of each node in the string.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the left node sibling (null when there is no left node
    /// sibling because this is the first node).
    /// </summary>
    public CharNode? LeftNode { get; set; }

    /// <summary>
    /// Gets the features attached to this node.
    /// </summary>
    public List<Feature> Features { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharNode"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ordinal">The ordinal.</param>
    public CharNode(int id, int ordinal)
    {
        Id = id;
        Ordinal = ordinal;
        Features = [];
    }

    /// <summary>
    /// Copies data (except ID and ordinal) from the specified node.
    /// </summary>
    /// <param name="node">The source node.</param>
    public void CopyFrom(CharNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        Value = node.Value;
        IsDeleted = node.IsDeleted;
        LeftNode = node.LeftNode;
        Features = node.Features;
        _prevValues = node._prevValues?.ToList();
        _coords = node._coords?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Adds the specified coordinate.
    /// </summary>
    /// <param name="name">The coordinate name.</param>
    /// <param name="value">The coordinate value.</param>
    public void AddCoord(string name, object value)
    {
        _coords ??= [];
        _coords[name] = value;
    }

    /// <summary>
    /// Gets the names of all the coords set for this character.
    /// </summary>
    /// <returns>Names, sorted ascending.</returns>
    public IList<string> GetCoordNames()
    {
        return _coords == null
            ? Array.Empty<string>()
            : (IList<string>)[.. _coords.Keys.OrderBy(s => s)];
    }

    /// <summary>
    /// Determines whether this character has the specified coordinate.
    /// </summary>
    /// <param name="name">The coordinate name.</param>
    /// <returns>
    ///   <c>true</c> if the specified name has coord; otherwise, <c>false</c>.
    /// </returns>
    public bool HasCoord(string name) => _coords?.ContainsKey(name) == true;

    /// <summary>
    /// Gets the coordinate with the specified name.
    /// </summary>
    /// <typeparam name="T">The type of the coordinate to cast value to.</typeparam>
    /// <param name="name">The coordinate name.</param>
    /// <returns>Value or default if not found.</returns>
    public T? GetCoord<T>(string name)
    {
        if (_coords == null) return default;
        object v = _coords[name];
        return v == null ? default : (T)v;
    }

    /// <summary>
    /// Deeply clones this node.
    /// </summary>
    /// <returns>New node.</returns>
    public virtual CharNode Clone()
    {
        return new CharNode(Id, Ordinal)
        {
            Value = Value,
            IsDeleted = IsDeleted,
            LeftNode = LeftNode,
            Features = Features,
            _prevValues = _prevValues?.ToList(),
            _coords = _coords?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{Ordinal}] '{Value}'" +
            (IsDeleted ? "*" : "");
    }
}
