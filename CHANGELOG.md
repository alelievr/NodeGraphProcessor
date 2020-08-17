# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0]

### Fixed
- Fixed relay node packing workflow and some deletion issues

## [0.8.0]

### Added
- Depth first compute order (enabled by default) instead of breadth first
- Cycle detection in the graph, cyclic nodes are now marked with a compute order of -2
- Added a node create menu when dropping an edge in the graph that also connects the edge to the new node (like in ShaderGraph or VFX Graph)
- Added ITypeAdapter.GetIncompatibleTypes to list all the incompatible types (prevent automatic casting / unwanted connectable ports, can be used to exclude a conversion to System.Object for example)
- Added a node inspector to show advanced settings in the inspector, see `ShowInInspector.cs` and `DrawDefaultInspector(bool fromInspector)` for more details.

### Changed
- Improved a lot the edge connection behavior for both input and output ports
- Improved domain reload performances by using `TypeCache` when possible
- Node copy/pasting now keeps the connected edges
- Copy/paste now works between multiple graph windows
- Copy/paste now supports node groups

### Fixed
- Fixed delete of multiple relay node at the same time breaking the graph
- Fixed undo event not being unregistered when closing the graph
- Fixed undo on parameter creation / deletion
- Fixed toolbar AddButtons deleting existing buttons

## [0.7.1]

### Added
- VisibleIf attribute in nodes, allow you to show fields only when another field have a specific value.
- Added the possibility to hide controls when the mouse is not over the node

## [0.7.0]

### Added
- Added a method to call the onProcess callback in the graph
- Support of multiple [NodeMenuItemAttribute] on the same class

### Fixed
- Fixed execution error in player when using IL2CPP
- Fixed ObjectField creation with FieldFactory

## [0.6.0]

### Added
- Added Relay nodes
- Added API to have a custom Edge Listener

### Changed
- When a port is disconnected, it's value is set to null or default.

### Fixed
- Window menu graph example

## [0.5.0]

### Added

- Added StackNode with custom view
- Added an API to notify the graph that a node have changed (BaseGraph.NotifyNodeChanged)

## [0.4.0]

### Changed

- Renamed Comment Block by Group

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
