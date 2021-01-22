# Upgrade to 1.0.0

The 1.0.0 version of NodeGraphProcessor brings a big change in the serialization system: we removed the JSON serialization system and replaced it by the new [SerializeReference](https://docs.unity3d.com/ScriptReference/SerializeReference.html) attribute.

**The minimum required version is Unity 2020.2.**

:warning: Before upgrading, **backup your data!** It's possible that you loose some data like parameters or nodes in the graph after migration. If you have a backup, you'll be able to patch the part of code that didn't upgraded well and the re-try to migrate.

## Which part was changed?

The list of nodes and parameters in the graph ScriptableObject is now stored using the [SerializeReference] attribute instead of relying on the old JSON serialization tech.

This changes will allow us to correctly serialize references of GameObject, having custom property drawers for exposed properties and in the future, use SerializedFields instead of the fieldFactory to display the fields in the nodes (which will bring the support of all field atributes like [Min], [Range], ect.)

## When does the migration occurs?

When the graph is deserialized. So it can be either wheen you click on it in the project window, open it or load it from a script in the editor.

## What can break while upgrading

Parameters are the most impacted by this change, now you need to write your own parameter class that inherits from `ExposedParameter` to be able to expose a parameter. Like this:

```CSharp
    [System.Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        [SerializeField] GameObject val;

        public override object value { get => val; set => val = (GameObject)value; }
        public override Type GetValueType() => typeof(GameObject);
    }
```

You can see the implementation of most common types `ExposedParameter.cs`, and you can still use `GetExposedParameterTypes` in `ExposedParameterView` to filter which exposed parameter is allowed in your graph.

There is an automatic upgrade path for parameters, but if you have a parameter on a custom type (struct or class), then it won't migrate automatically. So in this situation you can: backup your date first, then upgrade (so the graph breaks), add your exposed parameters in the code and roll back your graph data to the upgrade is triggered again.

## Why is the minimum required version Unity 2020.2 for this version?

Mainly because the [SerializeReference] feature was very bugged in versions prior to 2020.2, so it wasn't possible to do this change.

Previous versions of NodeGraphProcessor are still available in OpenUPM so you can still stay on an older version if you need it.
