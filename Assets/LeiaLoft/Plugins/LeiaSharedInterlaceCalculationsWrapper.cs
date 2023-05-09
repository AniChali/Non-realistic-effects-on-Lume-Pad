using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace LeiaLoft
{
	/// <summary>
	/// This class will try to pipe GetInterlaceMatrix calls to an artifact / Plugin. Artifacts must exist on OSX, Windows 32, Windows 64, and Android.
	/// </summary>
	public class LeiaSharedInterlaceCalculationsWrapper : AbstractArtifactWrapper
	{
		private const string dllName = "LeiaSharedInterlaceCalculations";

		[DllImport(dllName)]
		static extern void insertInterlaceMatrixByStepRates(
			int w, int h, int numViewsH,
			int viewStepRatePerUVX, int viewStepRatePerUVY, int viewStepRatePerRGB,
			float[] interlace_matrix, int interlace_matrix_size, float[] interlace_vector, int interlace_vector_size);

		/// <summary>
        /// Calculates an interlacing matrix
        /// </summary>
        /// <param name="panelWidth">App's width in pixels</param>
        /// <param name="panelHeight">App's height in pixels</param>
        /// <param name="numViewsHorizontal">Number of views in total</param>
        /// <param name="viewStepRateUVX">Number of view indices to step forward by as UV.x increases by 1 pixel</param>
        /// <param name="viewStepRateUVY">Number of view indices to step forward by as UV.y increases by 1 pixel</param>
        /// <param name="viewStepRateRGB">Number of view indices to step forward by as we move through R/G/B channels</param>
        /// <returns>A 16-element float[] array which represents an interlacing matrix. The interlacing vector can be assumed to be {0,0,0,0}</returns>
        public float[] GetInterlaceMatrix(int panelWidth, int panelHeight, int numViewsHorizontal,
			int viewStepRateUVX, int viewStepRateUVY, int viewStepRateRGB
			)
        {
			const int matrixDim = 4;
			float[] imat = new float[matrixDim * matrixDim];
			float[] ivec = new float[matrixDim];

			// On OSX, Unity will load this symbol from a .bundle
			// On Android, Unity will load this symbol from a .so which has been made with Android Studio
			// On Win in 32-bit build, Unity will load this symbol from a .DLL
			// On Win in 64-bit build, Unity will load this symbol from a .DLL
			insertInterlaceMatrixByStepRates(panelWidth, panelHeight, numViewsHorizontal,
				viewStepRateUVX, viewStepRateUVY, viewStepRateRGB,
				imat, imat.Length, ivec, ivec.Length);

			return imat;
        }
	}
}
