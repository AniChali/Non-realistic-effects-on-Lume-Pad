using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    public class DisplayConfigCalculator
    {
        /// <summary>
        /// Calculate a float[] which represents an interlacing matrix. This function supports more commonly used parameters
        /// </summary>
        /// <param name="interlacedWidth">Panel width</param>
        /// <param name="interlacedHeight">Panel height</param>
        /// <param name="numViewsHorizontal">Camera grid horizontal size</param>
        /// <param name="numViewsVertical">Camera grid vertical size</param>
        /// <param name="viewStepRatePerUVX">Number of viewIndices to increase by for each uv.x + 1.0 / panelWidth</param>
        /// <param name="viewStepRatePerUVY">Number of viewIndices to increase by for each uv.y + 1.0 / panelHeight</param>
        /// <param name="viewStepRatePerRGB">Number of viewIndices to increase by for each RGB index within [uv.x, uv.y]</param>
        /// <returns>A float[] which can be serialized or converted to Matrix4x4</returns>
        public static float[] CalculateInterlaceMatrix(int interlacedWidth, int interlacedHeight,
            int numViewsHorizontal, int numViewsVertical,
            int viewStepRatePerUVX, int viewStepRatePerUVY, int viewStepRatePerRGB)
        {
            const int matrixDim = 4;
            const int rgbCount = 3;
            const float minDiv = 0.1f;
            float[] data = new float[matrixDim * matrixDim];

            // start off with identity matrix
            for (int i = 0; i < matrixDim * matrixDim; i = i + matrixDim + 1)
            {
                data[i] = 1.0f;
            }
            // knock out last index
            data[matrixDim * matrixDim - 1] = 0.0f;

            // for vector (uv.x, uv.y, rgbIndex, viewIndex) set varyings of viewIndex with respect to uv.x, uv.y, rgb
            int UVXInd = matrixDim * 3 + 0;
            int UVYInd = matrixDim * 3 + 1;
            int RGBInd = matrixDim * 3 + 2;

            float viewCount = Mathf.Max(numViewsHorizontal * numViewsVertical, minDiv);

            data[UVXInd] = interlacedWidth * viewStepRatePerUVX * 1.0f / viewCount;
            data[UVYInd] = interlacedHeight * viewStepRatePerUVY * 1.0f / viewCount;
            data[RGBInd] = viewStepRatePerRGB * rgbCount * 1.0f / viewCount;

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interlaced_width">Width of the interlaced image in pixels.</param>
        /// <param name="interlaced_height">Height of the interlaced image in pixels.</param>
        /// <param name="display_alignment_offset">
        /// </param>
        /// <param name="num_views">The number of views.</param>
        /// <param name="view_id_increases_with_x">
        /// Toggles wether the view id's increase from left to right or right to left.
        /// Flipping this parameter together with view_id_increases_with_channel effectively reverses the view order.
        /// </param>
        /// <param name="slant">Toggles wether view id's increase or decrease with y.</param>
        /// <param name="reverse_view_order">Should the view order be reversed.</param>
        /// <param name="mirror_x_per_view">Should each view be mirrored in x.</param>
        /// <param name="mirror_y_per_view">Should each view be mirrored in y</param>
        /// <param name="interlaceMatrix">The calculated interlace matrix.</param>
        /// <param name="interlaceVector">The calculated interlace vector.</param>
        public static void CalculateInterlaceMatrix(int interlaced_width,
                                                    int interlaced_height,
                                                    int display_alignment_offset,
                                                    int num_views,
                                                    bool slant,
                                                    bool reverse_view_order,
                                                    bool mirror_x_per_view,
                                                    bool mirror_y_per_view,
                                                    out Matrix4x4 interlace_matrix,
                                                    out Vector4 interlace_vector)
        {
            Matrix4x4 non_normalized_interlace_matrix = Matrix4x4.identity;
            Vector4 non_normalized_interlace_vector = Vector4.zero;

            Vector4 interlaced_resolution = new Vector4(interlaced_width, interlaced_height, 3, num_views);

            non_normalized_interlace_matrix.m30 = 3.0f;
            non_normalized_interlace_matrix.m31 = 1.0f;
            non_normalized_interlace_matrix.m32 = 1.0f;
            non_normalized_interlace_matrix.m33 = 0.0f;

            non_normalized_interlace_vector[3] = (float)display_alignment_offset;

            if (!slant)
            {
                non_normalized_interlace_matrix.m31 *= -1.0f;
            }

            if (reverse_view_order)
            {
                non_normalized_interlace_matrix.m30 *= -1.0f;
                non_normalized_interlace_matrix.m31 *= -1.0f;
                non_normalized_interlace_matrix.m32 *= -1.0f;
                non_normalized_interlace_vector[3] = num_views - 1.0f - non_normalized_interlace_vector[3];
            }

            if (mirror_x_per_view)
            {
                non_normalized_interlace_matrix.m00 = -1.0f;
                non_normalized_interlace_vector[0] = (float)interlaced_width - 1.0f;
            }

            if (mirror_y_per_view)
            {
                non_normalized_interlace_matrix.m11 = -1.0f;
                non_normalized_interlace_vector[1] = (float)interlaced_height - 1.0f;
            }

            PostMultiplyRGBAA(ref non_normalized_interlace_matrix, ref non_normalized_interlace_vector);

            // Normalize to unit cube.
            NormalizeInterlaceMatrix(interlaced_resolution, non_normalized_interlace_matrix, non_normalized_interlace_vector, out interlace_matrix, out interlace_vector);
        }

        static void NormalizeInterlaceMatrix(Vector4 interlace_resolution, Matrix4x4 non_normalized_matrix, Vector4 non_normalized_vector, out Matrix4x4 normalized_matrix, out Vector4 normalized_vector)
        {
            normalized_matrix = Matrix4x4.identity;
            normalized_vector = Vector4.zero;
            for (int i = 0; i < 4; ++i)
            {
                Vector4 non_normalized_row = non_normalized_matrix.GetRow(i);
                Vector4 normalized_row = Vector4.zero;
                for (int j = 0; j < 4; ++j)
                {
                    normalized_row[j] = non_normalized_row[j] * interlace_resolution[j] / interlace_resolution[i];
                }
                normalized_matrix.SetRow(i, normalized_row);
                normalized_vector[i] = non_normalized_vector[i] / interlace_resolution[i];
            }
        }

        static void PostMultiplyRGBAA(ref Matrix4x4 non_normalized_interlace_matrix, ref Vector4 non_normalized_interlace_vector)
        {
            // Calculate RBG anti-aliasing matrix that shifts red and blue components to sample from the same xy-position as their corresponding green component.
            Matrix4x4 rgb_antialiasing_matrix = Matrix4x4.identity;
            Vector4 rgb_antialiasing_vector = Vector4.zero;

            float min_magnitude = 999.0f;
            Vector2 green_to_blue = Vector2.zero;
            for (green_to_blue.x = -2; green_to_blue.x <= 2; ++green_to_blue.x)
            {
                green_to_blue.y = -green_to_blue.x * (non_normalized_interlace_matrix.m30 / non_normalized_interlace_matrix.m31) - (non_normalized_interlace_matrix.m32 / non_normalized_interlace_matrix.m31);
                if (isFloatAnInteger(green_to_blue.y, 0.001f))
                {
                    if (green_to_blue.magnitude < min_magnitude)
                    {
                        min_magnitude = green_to_blue.magnitude;
                        rgb_antialiasing_matrix.m02 = -green_to_blue.x;
                        rgb_antialiasing_matrix.m12 = -green_to_blue.y;
                        rgb_antialiasing_vector[0] = green_to_blue.x;
                        rgb_antialiasing_vector[1] = green_to_blue.y;
                    }
                }
            }

            // Post-multiply interlace matrix by rgb AA matrix.
            non_normalized_interlace_matrix = rgb_antialiasing_matrix * non_normalized_interlace_matrix;
            non_normalized_interlace_vector = rgb_antialiasing_matrix * non_normalized_interlace_vector + rgb_antialiasing_vector;
        }

        static bool isFloatAnInteger(float x, float epsilon)
        {
            return Mathf.Abs(x - Mathf.Round(x)) < epsilon;
        }

        public static List<float> convertActBetaToShaderCoeffs(List<float> ActCoefficients, float beta)
        {
            List<float> shaderCoeffs = new List<float>();
            shaderCoeffs.Add(1.0f);
            shaderCoeffs.AddRange(ActCoefficients);

            float normalizer = 1.0f;
            foreach (float a in ActCoefficients)
            {
                normalizer -= beta * a;
            }

            for (int i = 0; i < shaderCoeffs.Count; ++i)
            {
                shaderCoeffs[i] /= normalizer;
            }

            return shaderCoeffs;
        }
    }

    public static class DisplayConfigCalculationUtils
    {
        private const int dim0 = 4;

        /// <summary>
        /// Conversion from row-major Matrix4x4 to float[]
        /// </summary>
        /// <param name="mat">A row-major matrix</param>
        /// <returns>Elements of the matrix, in an array</returns>
        public static float[] ToFloatArray(this Matrix4x4 mat)
        {
            float[] floatData = new float[dim0 * dim0];

            for (int i = 0; i < dim0; ++i)
            {
                for (int j = 0; j < dim0; ++j)
                {
                    floatData[i * dim0 + j] = mat[i, j];
                }
            }
            return floatData;
        }

        /// <summary>
        /// Given a 16-element array of floats, convert it to a 4x4 matrix
        /// </summary>
        /// <param name="flatMat">A float[] which has data ordered [00 01 02 03 10 11 12 13 20 21 22 23 30 31 32 33]</param>
        /// <returns>A row-major Matrix4x4 constructed from flatMat data</returns>
        public static Matrix4x4 ToMatrix4x4(this float[] flatMat)
        {
            Matrix4x4 matrix = Matrix4x4.zero;
            for (int row = 0; row < dim0; ++row)
            {
                matrix.SetRow(row, new Vector4(flatMat[4 * row + 0], flatMat[4 * row + 1], flatMat[4 * row + 2], flatMat[4 * row + 3]));
            }
            return matrix;
        }

        /// <summary>
        /// Conversion from Vector4 to float[]
        /// </summary>
        /// <param name="vec">A vector with 4 elements</param>
        /// <returns>A float[] with 4 elements</returns>
        public static float[] ToFloatArray(this Vector4 vec)
        {
            float[] floatData = new float[dim0];
            for (int i = 0; i < dim0; ++i)
            {
                floatData[i] = vec[i];
            }

            return floatData;
        }

        /// <summary>
        /// Given a 4-element array of floats, convert it to a Vector4
        /// </summary>
        /// <param name="array">A 4-element float array</param>
        /// <returns>A Vector4</returns>
        public static Vector4 ToVector4(this float[] array)
        {
            Vector4 vec = Vector4.zero;
            for (int i = 0; i < array.Length && i < dim0; ++i)
            {
                vec[i] = array[i];
            }
            return vec;
        }
    }
}
