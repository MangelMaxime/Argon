# Custom SerializationBinder

This sample creates a custom `System.Runtime.Serialization.SerializationBinder` that writes only the type name when including type data in JSON.

<!-- snippet: SerializeSerializationBinderTypes -->
<a id='snippet-SerializeSerializationBinderTypes'></a>
```cs
public class KnownTypesBinder : ISerializationBinder
{
    public IList<Type> KnownTypes { get; set; }

    public Type BindToType(string assemblyName, string typeName) =>
        KnownTypes.SingleOrDefault(t => t.Name == typeName);

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = null;
        typeName = serializedType.Name;
    }
}

public class Car
{
    public string Maker { get; set; }
    public string Model { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/SerializeSerializationBinder.cs#L7-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializeSerializationBinderTypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeSerializationBinderUsage -->
<a id='snippet-SerializeSerializationBinderUsage'></a>
```cs
var knownTypesBinder = new KnownTypesBinder
{
    KnownTypes = new List<Type> {typeof(Car)}
};

var car = new Car
{
    Maker = "Ford",
    Model = "Explorer"
};

var json = JsonConvert.SerializeObject(car, Formatting.Indented, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Objects,
    SerializationBinder = knownTypesBinder
});

Console.WriteLine(json);
// {
//   "$type": "Car",
//   "Maker": "Ford",
//   "Model": "Explorer"
// }

var newValue = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Objects,
    SerializationBinder = knownTypesBinder
});

Console.WriteLine(newValue.GetType().Name);
// Car
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/SerializeSerializationBinder.cs#L34-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializeSerializationBinderUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
