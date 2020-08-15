### Upgrade to 1.0.0

The 1.0.0 version of NodeGraphProcessor brings a big change in the serialization system: we removed the JSON serialization system and replaced it by the new [SerializeReference](https://docs.unity3d.com/ScriptReference/SerializeReference.html) attribute.

#### Which part was changed?

TODO nodes and parameters

#### Why?

TODO

#### What can break while upgrading

#### How to debug the upgrade of a graph


Only node and parameters are subject to this changes

There is an automatic migration path that will convert all your nodes and parameters to the new serialization format but some things can go wrong:
- 

Don't use class initializator, they are broken

Note that we still keep the JSON serialization for copy and paste in the graph editor.