using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    /// <summary>
    /// Implement this interface on a BaseNode, it allows you to automatically spawn a node if an asset of type T is dropped in the graphview area
    /// </summary>
    /// <typeparam name="T">The type object your node will be created from, it must be a subclass of UnityEngine.Object</typeparam>
    public interface ICreateNodeFrom<T> where T : Object
    {
        /// <summary>
        /// This function is called just after creating the node from an object and allows you to initialize the node with the object data.
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>True if the initialization happened correctly. False otherwise, returning false will discard your node.</returns>
        bool InitializeNodeFromObject(T value);
    }
}