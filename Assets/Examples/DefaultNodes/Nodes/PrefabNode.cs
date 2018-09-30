<<<<<<< HEAD
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Prefab")]
public class PrefabNode : BaseNode
{
	[Output(name = "Out")]
	public GameObject		output;

	public override string		name => "Prefab";
}
=======
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Prefab")]
public class PrefabNode : BaseNode
{
	[Output(name = "Out")]
	public GameObject		output;

	public override string		name => "Prefab";
}
>>>>>>> 85ff4ace9e4636013762a222efd3312ae30ff3ce
