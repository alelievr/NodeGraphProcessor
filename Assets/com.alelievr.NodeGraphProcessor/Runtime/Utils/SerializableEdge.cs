using System;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class SerializableEdge : ISerializationCallbackReceiver
    {
        #region Fields
        public string GUID;

        [SerializeField] private BaseGraph _owner;
        [SerializeField] private string _inputNodeGUID;
        [SerializeField] private string _outputNodeGUID;
        [NonSerialized] public BaseNode _inputNode;
        [NonSerialized] public NodePort _inputPort;
        [NonSerialized] public NodePort _outputPort;

        //temporary object used to send port to port data when a custom input/output function is used.
        [NonSerialized]
        public object passThroughBuffer;

        [NonSerialized]
        public BaseNode _outputNode;

        public string inputFieldName;
        public string outputFieldName;

        // Use to store the id of the field that generate multiple ports
        public string inputPortIdentifier;
        public string outputPortIdentifier;
        #endregion

        #region ISerializationCallbackReceiver Members
        public void OnBeforeSerialize()
        {
            if (_outputNode == null || _inputNode == null)
            {
                return;
            }

            _outputNodeGUID = _outputNode.GUID;
            _inputNodeGUID = _inputNode.GUID;
        }

        public void OnAfterDeserialize() { }
        #endregion

        #region Methods
        public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
        {
            var edge = new SerializableEdge();

            edge._owner = graph;
            edge.GUID = Guid.NewGuid().ToString();
            edge._inputNode = inputPort.owner;
            edge.inputFieldName = inputPort.fieldName;
            edge._outputNode = outputPort.owner;
            edge.outputFieldName = outputPort.fieldName;
            edge._inputPort = inputPort;
            edge._outputPort = outputPort;
            edge.inputPortIdentifier = inputPort.portData.identifier;
            edge.outputPortIdentifier = outputPort.portData.identifier;

            return edge;
        }

        //here our _owner have been deserialized
        public void Deserialize(BaseGraph owner)
        {
            _owner = owner;
            if (!owner.nodesPerGUID.ContainsKey(_outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(_inputNodeGUID))
            {
                return;
            }

            _outputNode = owner.nodesPerGUID[_outputNodeGUID];
            _inputNode = owner.nodesPerGUID[_inputNodeGUID];
            _inputPort = _inputNode.GetPort(inputFieldName, inputPortIdentifier);
            _outputPort = _outputNode.GetPort(outputFieldName, outputPortIdentifier);
        }

        public override string ToString()
        {
            return $"{_outputNode.name}:{_outputPort.fieldName} -> {_inputNode.name}:{_inputPort.fieldName}";
        }
        #endregion
    }
}