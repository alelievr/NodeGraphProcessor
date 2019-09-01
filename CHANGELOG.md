# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0]

### Fixed
- Fixed Enums fields created via FieldFactory (inspector graph UI for properties should now work with enums)

### Added
- User defined type conversions inside the graph (ex: float to vector). See TypeAdapter.cs

## [0.2.0]

### Fixed
- Fixed build errors
- Fixed badge distance when node used a custom size

## [0.1.0]

### Added
- API to create C# template menu items (and moved the default Node and NodeView templates to Examples)
- Added an event that is fired when something is changed inside the graph (BaseGraph.onGraphChanges)
- Added support of node messages (you can attach one message with an icon to a node, either from the process function or from the view. See `AddMessage` and `AddMessageView` functions)
![image](https://user-images.githubusercontent.com/6877923/63230798-07593f00-c212-11e9-92ea-ec3ba3c11ba0.png)


### Fixed
- When switching a port type, the connected edges are now removed if the new port type is incompatible.

## [0.0.0]

### Added

- Node and Graph property serialization (as json) Scriptable Object to store graph as a Unity asset.
- Highly customizable and simple node and links API
- Support multi-input into a container (multiple float into a list of float for example)
- Graph processor which execute node's logic with a - dependency order
- [Powerful C# API to add new nodes / graphs](https://github.com/alelievr/NodeGraphProcessor/wiki/Node-scripting-API)
- Exposed parameters that can be set per-asset to customize the graph processing from scripts or the inspector
- Search window to create new nodes
- Colored comment blocks (node groups)
