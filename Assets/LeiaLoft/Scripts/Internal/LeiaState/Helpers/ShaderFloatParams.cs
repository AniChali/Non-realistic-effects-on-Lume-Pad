/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// Just to avoid lot of strings with material.SetFloat calls. <see cref="ShaderFloatParams.ApplyTo"/> method.
    /// </summary>
    class ShaderFloatParams
    {
#pragma warning disable 0649 // Suppress warning that var is never assigned to and will always be null
        public float _width = 3840;
        public float _height = 2160;
        public float _viewResX = 1680;
        public float _viewResY = 945;
        public float _viewsX = 8;
        public float _viewsY = 1;
        public float _offsetX = 0;
        public float _offsetY = 0;
        public float _viewRectX = 0;
        public float _viewRectY = 0;
        public float _viewRectW = 1;
        public float _viewRectH = 1;
        public float _adaptFOVx = 0;
        public float _adaptFOVy = 0;
        public float _orientation = 0;
        public float _showCalibrationSquares = 0;
        public float _enableSwizzledRendering = 1;
        public float _enableHoloRendering = 1;
        public float _enableSuperSampling = 0;
        public float _separateTiles = 0;
        public Matrix4x4 _interlace_matrix;
        public float[] _deltaXArray = { 0, 0, 0, 0, 0, 0, 0, 0 };
        public int _deltaXArraySize;

        public Vector4 _interlace_vector;
        public float _isFlippedAlignment = 0;
#pragma warning restore 0649

        /// <summary>
        /// Sends all float fields to a material using SetFloat method, field name and value
        /// </summary>
        public void ApplyTo(Material material)
        {
            var fields = this.GetType().GetFields();

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(float))
                {
                    material.SetFloat(field.Name, (float)field.GetValue(this));
                }
                else if (field.FieldType == typeof(Matrix4x4))
                {
                    material.SetMatrix(field.Name, (Matrix4x4)field.GetValue(this));
                }
                else if (field.FieldType == typeof(Vector4))
                {
                    material.SetVector(field.Name, (Vector4)field.GetValue(this));
                }
                else if (field.FieldType == typeof(int))
                {
                    material.SetInt(field.Name, (int)field.GetValue(this));
                }
                else if (field.FieldType == typeof(float[]))
                {
                    material.SetFloatArray(field.Name, (float[])field.GetValue(this));
                }
                else
                {
                    continue;
                }
            }
        }
    }

}