# NodeGraphProcessor
Node graph editor framework focused on data processing using Unity UIElements and C# 4.7

### Unity Compatible versions

This project requires at least Unity **2019.1** with a scripting runtime version of 4.x in player settings.
The current Unity version used for the project is **2019.1b04**

### Installation

There are two ways to install this asset: you can use the Unity package manager or move the entire repo inside your Assets folder.
To install using the package manager:

- download this repo
- inside the package manager click the '+' button at the bottom to add a package from disk
- then select the package.json file located in `Assets/NodeGraphProcessor`
- package is installed :)

Note that you'll not have access to the examples provided in this repo because the package only include the core of NodeGraphProcessor.

### Features

- Node and Graph property serialization (as json)
- Scriptable Object to store graph as a Unity asset.
- Highly customizable and simple node and links API
- Support multi-input into a container (multiple float into a list of float for example)
- Graph processor which execute node's logic with a dependency order
- [Powerful C# API to add new nodes / graphs](https://github.com/alelievr/NodeGraphProcessor/wiki/Node-scripting-API)
- Exposed parameters that can be set per-asset to customize the graph processing from scripts or the inspector

### Remaining to do

- Investigate for ECS/Jobs integration
- More examples
- Fast node creation menu
- Node collapse feature
- API to create the graph
- Runtime processing without the editor


For more details consult our [trello](https://trello.com/b/Xk4rfnuV/node-graph-processor).

### Screens

![](https://preview.ibb.co/hP0CvT/Screen_Shot_2018_06_24_at_18_05_50.png)
![](https://image.noelshack.com/fichiers/2018/35/7/1535906391-graph.png)
![](http://g.recordit.co/U1MAlFfuba.gif)
![image](https://user-images.githubusercontent.com/6877923/53634256-0445a480-3c1a-11e9-99e5-d8f3616863bd.png)
