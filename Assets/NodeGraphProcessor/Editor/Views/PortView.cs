using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System;

namespace GraphProcessor
{
	class PortView : Port
	{
		public PortView(Orientation portOrientation, Direction portDirection, Type type, EdgeConnectorListener edgeConnectorListener) : base(portOrientation, portDirection, type)
		{
			AddStyleSheetPath("Styles/PortView");

			m_EdgeConnector = new EdgeConnector< Edge >(edgeConnectorListener);

			this.AddManipulator(m_EdgeConnector);

			portName = "Test";

			visualClass = "typeVector2";

			//TODO: set the visualClass using the field data type
		}

	}
}