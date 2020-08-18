// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor.UIElements;
// using UnityEditor.Experimental.GraphView;
// using UnityEngine.UIElements;
// using GraphProcessor;

// [NodeCustomEditor(typeof(ConvertNode))]
// public class ConvertNodeView : BaseNodeView
// {
//     protected override bool hasSettings => true;

//     private ConvertNode dataNode;


//     public override void Enable()
//     {
//         dataNode = nodeTarget as ConvertNode;

//         var parameters = dataNode.ParameterInfos;
//         foreach (var parameter in parameters)
//         {
//             if (parameter.HasDefaultValue)
//             {
//                 if (parameter.ParameterType == typeof(Vector4))
//                 {
//                     var fieldElement = new Vector4Field();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Vector4)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(Vector3))
//                 {
//                     var fieldElement = new Vector3Field();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Vector3)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(Vector2))
//                 {
//                     var fieldElement = new Vector2Field();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Vector2)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(Rect))
//                 {
//                     var fieldElement = new RectField();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Rect)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(Color))
//                 {
//                     var fieldElement = new ColorField();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Color)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(AnimationCurve))
//                 {
//                     var fieldElement = new CurveField();
//                     if (parameter.DefaultValue != null) fieldElement.value = (AnimationCurve)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else if (parameter.ParameterType == typeof(Gradient))
//                 {
//                     var fieldElement = new GradientField();
//                     if (parameter.DefaultValue != null) fieldElement.value = (Gradient)parameter.DefaultValue;
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//                 else
//                 {
//                     var fieldElement = new TextField();
//                     if (parameter.DefaultValue != null) fieldElement.value = parameter.DefaultValue.ToString();
//                     else  fieldElement.value = "Please overwrite ToString()";
//                     fieldElement.label = parameter.Name;
//                     contentContainer.Add(fieldElement);
//                 }
//             }
//         }

//     }

// }