# NodeGraphProcessor
Node graph editor framework focused on data processing using Unity UIElements, GraphView and C# 4.7

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4c62ece874d14a0b965b92cb163e3146)](https://www.codacy.com/manual/alelievr/NodeGraphProcessor?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=alelievr/NodeGraphProcessor&amp;utm_campaign=Badge_Grade)
[![openupm](https://img.shields.io/npm/v/com.alelievr.node-graph-processor?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.alelievr.node-graph-processor/)

This node based solution provides a great C# API allowing you to implement conditional graphs, dependencies graphs, processing graphs and more.  
![image](https://user-images.githubusercontent.com/6877923/83576832-f2486500-a532-11ea-9d2a-a6b75b980813.png)

Based on Unity's GraphView technology, NodeGraphProcessor is also very fast and works well with large graphs.  
![Performance](https://user-images.githubusercontent.com/6877923/83576843-f70d1900-a532-11ea-80fb-c8fede6aa7ed.gif)

Simple and powerful C# node API to create new nodes and custom views.

```CSharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Operations/Sub")] // Add the node in the node creation context menu
public class SubNode : BaseNode
{
    [Input(name = "A")]
    public float                inputA;
    [Input(name = "B")]
    public float                inputB;

    [Output(name = "Out")]
    public float				output;

    public override string		name => "Sub";

    // Called when the graph is process, process inputs and assign the result in output.
    protected override void Process()
    {
        output = inputA - inputB;
    }
}
```

### Unity Compatible versions

This project requires at least Unity **2019.3** with a scripting runtime version of 4.x in player settings.
The current Unity version used for the project is **2019.3.12f1**

### Installation

#### Install Manually
There are two ways to install this asset: you can use the Unity package manager or move the entire repo inside your Assets folder.
To install using the package manager:

- download this repo
- inside the package manager click the '+' button at the bottom to add a package from disk
- then select the package.json file located in `Assets/NodeGraphProcessor`
- package is installed :)

#### Install via OpenUPM

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.alelievr.node-graph-processor
```

#### Install via Git

Alternatively, you can use the [git address feature in the package manager](https://forum.unity.com/threads/git-support-on-package-manager.573673/) on the branch [#upm](https://github.com/alelievr/NodeGraphProcessor/tree/upm), it only contains the package but it may be out of sync compared to master.

Note that you'll not have access to the examples provided in this repo because the package only include the core of NodeGraphProcessor.

### Features

- Node and Graph property serialization (as json)
- Scriptable Object to store graph as a Unity asset.
- Highly customizable and simple node and links API
- Support multi-input into a container (multiple float into a list of float for example)
- Graph processor which execute node's logic with a dependency order
- [Documented C# API to add new nodes / graphs](https://github.com/alelievr/NodeGraphProcessor/wiki/Node-scripting-API)
- Exposed parameters that can be set per-asset to customize the graph processing from scripts or the inspector
- Parameter set mode, you can now output data from thegraph using exposed parameters. Their values will be updated when the graph is processed
- Search window to create new nodes
- Colored groups
- Node messages (small message with it's icon beside the node)
- Stack Nodes
- Relay nodes
- Display additional settings in the inspector
- Node creation menu on edge drop
- Simplified edge connection compared to default GraphView (ShaderGraph and VFX Graph)
- Multiple graph window workflow (copy/paste)

More details are available [in the Changelog](CHANGELOG.md)

### Documentation

API doc is available here: [alelievr.github.io/NodeGraphProcessor](https://alelievr.github.io/NodeGraphProcessor/api/index.html)

The user manual is hosted using [Github Wiki](https://github.com/alelievr/NodeGraphProcessor/wiki).

### Remaining to do

- Investigate for ECS/Jobs integration
- API to create the graph in C#
- Subgraphs
- Use SerializedReference instead of Json (faster serialization + smaller assets)
- Sticky notes (requires Unity 2020.1)

For more details consult our [Github Project page](https://github.com/alelievr/NodeGraphProcessor/projects/2).

### Gallery

#### Minimap
![](https://user-images.githubusercontent.com/6877923/90036471-6043a200-dcc3-11ea-8702-9ccc62cb0f8a.gif)

#### Relay nodes
![](https://user-images.githubusercontent.com/6877923/89329982-e04c8500-d68f-11ea-8218-261225170978.gif)

#### Node connection menu
![](https://user-images.githubusercontent.com/6877923/89330117-12f67d80-d690-11ea-9b62-f878b86b8342.gif)

#### Node creation menu
![](https://user-images.githubusercontent.com/6877923/58935811-893adf80-876e-11e9-9f69-69ce51a432b8.png)

#### Graph Parameters
![](https://user-images.githubusercontent.com/6877923/90035202-d6470980-dcc1-11ea-92e0-a754820bdc55.png)

#### Groups
![](https://user-images.githubusercontent.com/6877923/58935692-3fea9000-876e-11e9-945e-8a874a4586a9.png)

#### Node Settings
![](https://user-images.githubusercontent.com/6877923/71757124-c34e9a00-2e93-11ea-900c-63ecd772af3f.gif)

#### Node Messages
![](https://user-images.githubusercontent.com/6877923/63230815-51dabb80-c212-11e9-9d54-382e649e77f1.png)

#### Conditional Processing (in Example)
![](https://user-images.githubusercontent.com/6877923/69500269-e469b580-0ef9-11ea-9c4b-f58e793f7ecd.gif)

#### Stacks
![](https://user-images.githubusercontent.com/6877923/71782933-25b4b100-2fe0-11ea-9b57-0198f7161535.gif)

#### Relay Node Packing
![](https://user-images.githubusercontent.com/6877923/77270201-808aaa00-6cab-11ea-9028-e671092be194.gif)

#### Node Inspector
![](https://user-images.githubusercontent.com/6877923/87306684-ac5ec380-c518-11ea-9346-1ed47e8cd016.gif)

#### Improved Edge Connection
![](https://user-images.githubusercontent.com/6877923/89890139-272c0480-dbd3-11ea-86f4-696d260f707b.gif)

#### Multi-Window support
![](https://user-images.githubusercontent.com/6877923/89891415-504d9480-dbd5-11ea-8b1d-873031a0677c.gif)
