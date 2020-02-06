# NodeGraphProcessor
Node graph editor framework focused on data processing using Unity UIElements and C# 4.7

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4c62ece874d14a0b965b92cb163e3146)](https://www.codacy.com/manual/alelievr/NodeGraphProcessor?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=alelievr/NodeGraphProcessor&amp;utm_campaign=Badge_Grade)
[![openupm](https://img.shields.io/npm/v/com.alelievr.node-graph-processor?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.alelievr.node-graph-processor/)

### Unity Compatible versions

This project requires at least Unity **2019.3** with a scripting runtime version of 4.x in player settings.
The current Unity version used for the project is **2019.3.0f6**

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
- [Powerful C# API to add new nodes / graphs](https://github.com/alelievr/NodeGraphProcessor/wiki/Node-scripting-API)
- Exposed parameters that can be set per-asset to customize the graph processing from scripts or the inspector
- Parameter set mode, you can now output data from thegraph using exposed parameters. Their values will be updated when the graph is processed
- Search window to create new nodes
- Colored groups
- Node messages (small message with it's icon beside the node)
- Stack Nodes

More details are available [in the Changelog](CHANGELOG.md)

### Documentation

API doc is available here: [alelievr.github.io/NodeGraphProcessor](https://alelievr.github.io/NodeGraphProcessor/api/index.html)

The user manual is hosted using [Github Wiki](https://github.com/alelievr/NodeGraphProcessor/wiki).

### Remaining to do

- Investigate for ECS/Jobs integration
- More examples
- API to create the graph
- Subgraphs
- Use SerializedReference instead of Json (faster serialization + smaller assets)
- Sticky notes

For more details consult our [trello](https://trello.com/b/Xk4rfnuV/node-graph-processor).

### Screens

![](https://preview.ibb.co/hP0CvT/Screen_Shot_2018_06_24_at_18_05_50.png)
![](https://image.noelshack.com/fichiers/2018/35/7/1535906391-graph.png)
![](http://g.recordit.co/U1MAlFfuba.gif)
![](https://user-images.githubusercontent.com/6877923/53634256-0445a480-3c1a-11e9-99e5-d8f3616863bd.png)
![](https://user-images.githubusercontent.com/6877923/58935692-3fea9000-876e-11e9-945e-8a874a4586a9.png)
![](https://user-images.githubusercontent.com/6877923/58935811-893adf80-876e-11e9-9f69-69ce51a432b8.png)
![](https://user-images.githubusercontent.com/6877923/71757124-c34e9a00-2e93-11ea-900c-63ecd772af3f.gif)
![](https://user-images.githubusercontent.com/6877923/63230815-51dabb80-c212-11e9-9d54-382e649e77f1.png)
![](https://user-images.githubusercontent.com/6877923/69500269-e469b580-0ef9-11ea-9c4b-f58e793f7ecd.gif)
![](https://user-images.githubusercontent.com/6877923/71782933-25b4b100-2fe0-11ea-9b57-0198f7161535.gif)

