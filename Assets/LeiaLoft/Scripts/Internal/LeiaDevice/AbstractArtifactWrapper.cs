using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LeiaLoft
{
    /// <summary>
    /// Defines functionality which (classes which call external libraries) should fulfill
    /// </summary>
	public interface IArtifactWrapper
    {
		void SetValue(string functionName, params object[] args);
		T GetValue<T>(string functionName, params object[] args);
	}

    /// <summary>
    /// Classes which inherit from AbstractArtifactWrapper can route their GetValue/SetValue calls through extern functions on the class out to linked artifacts.
    /// Currently only supports data exchange of strings, floats, and ints.
    ///
    /// If you wish to wrap around an artifact's API, choose the cleanest approach for your circumstances:
    /// - inherit from AbstractArtifactWrapper, or
    /// - have-a-AbstractArtifactWrapper object or
    /// - have-an-IArtifactWrapper-implementer
    /// </summary>
    public abstract class AbstractArtifactWrapper : IArtifactWrapper
    {
        const string failTag = "FAIL";

        private readonly Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        protected AbstractArtifactWrapper()
        {
            // reflect on class once
            MethodInfo[] methodArr = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            // cache available methods
            foreach (MethodInfo mInfo in methodArr)
            {
                methods[mInfo.Name] = mInfo;
            }
        }

        /// <summary>
        /// Given a name of a function which returns a pointer to an array of pointers which contain T[], move the data into C# as a FLAT T[]
        /// </summary>
        /// <typeparam name="T">Array type. Support int, float</typeparam>
        /// <param name="functionName">External function which returns an IntPtr[] where each IntPtr is a pointer to a T[]</param>
        /// <param name="arrCount">Number of arrays to look up</param>
        /// <param name="arrLen">Uniform length of arrays</param>
        /// <returns>An array of arrCount * arrLen with all elements of the T[][] of the extern function in the returned T[]</returns>
        public virtual T[] GetFlatNxMArray<T>(string functionName, int arrCount, int arrLen)
        {
            T[][] data2d = GetNxMArray<T>(functionName, arrCount, arrLen);

            T[] flat = new T[arrCount * arrLen];

            for (int i = 0; i < arrCount; ++i)
            {
                for (int j = 0; j < arrLen; ++j)
                {
                    flat[i * arrLen + j] = data2d[i][j];
                }
            }

            return flat;
        }

        /// <summary>
        /// Given a name of a function which returns a pointer to an array of pointers which contain T[], move the data into C# as a T[][]
        /// </summary>
        /// <typeparam name="T">Array type. Support int, float</typeparam>
        /// <param name="functionName">External function which returns an IntPtr[] where each IntPtr is a pointer to a T[]</param>
        /// <param name="arrCount">Number of arrays to look up</param>
        /// <param name="arrLen">Uniform length of arrays</param>
        /// <returns>An array of arrays of T</returns>
        public virtual T[][] GetNxMArray<T>(string functionName, int arrCount, int arrLen)
        {
            // start by populating a 1D array of IntPtrs. each IntPtr is a pointer to an array
            IntPtr[] ptrs = Get1DArray<IntPtr>(functionName, arrCount);

            T[][] data = new T[arrCount][];
            for(int i = 0; i < ptrs.Length; ++i)
            {
                data[i] = MarshalToGeneric<T>(ptrs[i], arrLen);
            }
            return data;
        }

        /// <summary>
        /// Given a name of a function which returns a pointer to an array T[], move the data into C# as a T[]
        /// </summary>
        /// <typeparam name="T">Array type. Support int, float, IntPtr</typeparam>
        /// <param name="functionName">External function which returns an IntPtr[] to a T[]</param>
        /// <param name="len">Number of elements in the array that we are pointing at</param>
        /// <param name="args">Additional args to provide in call</param>
        /// <returns>The data returned by the function, in a T[]</returns>
        public virtual T[] Get1DArray<T>(string functionName, int len, params object[] args)
        {
            if (methods.ContainsKey(functionName) && methods[functionName].ReturnType == typeof(IntPtr))
            {
                IntPtr ptr = (IntPtr)methods[functionName].Invoke(this, args);
                T[] data = MarshalToGeneric<T>(ptr, len);
                return data;
            }
            else
            {
                LogError(functionName, "No API member " + functionName + " or return type was not IntPtr", args);
                return default(T[]);
            }
        }

        /// <summary>
        /// Convenience function for calling an extern function and converting its returned IntPtr to a C# string
        /// </summary>
        /// <param name="functionName">External function to call</param>
        /// <param name="args">Additional args to provide in call</param>
        /// <returns>C#-accessible ANSI String equivalent of the function's returned pointer-to-char*</returns>
        public string GetString(string functionName, params object[] args)
        {
            if (methods.ContainsKey(functionName) && methods[functionName].ReturnType == typeof(IntPtr))
            {
                IntPtr ptr = (IntPtr)methods[functionName].Invoke(this, args);
                string ansiString = Marshal.PtrToStringAnsi(ptr);
                return ansiString;
            }
            else
            {
                LogError(functionName, "No API member " + functionName + " or return type was not IntPtr", args);
                return failTag;
            }
        }

        /// <summary>
        /// Retrieves the return result of the external function at "functionName"
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="functionName">Name of extern function on concrete class / in wrapped API which we will actually call</param>
        /// <param name="args">Additional args to pass to the external call</param>
        /// <returns>Return type specified by the user. Make sure your generic return type matches the return type that the wrapped API returns</returns>
        public virtual T GetValue<T>(string functionName, params object[] args)
        {
            // please make sure to only call an extern function from here. GetValue("GetValue") will cause an infinite loop
            if (methods.ContainsKey(functionName))
            {
                return (T)methods[functionName].Invoke(this, args);
            }
            else
            {
                LogError(functionName, "No API member " + functionName, args);
                return default(T);
            }
        }

        /// <summary>
        /// Calls a function "functionName" in an external library
        /// </summary>
        /// <param name="functionName">Name of extern function to call</param>
        /// <param name="args">Additional args to provide in extern function call</param>
        public virtual void SetValue(string functionName, params object[] args)
        {
            if (methods.ContainsKey(functionName))
            {
                methods[functionName].Invoke(this, args);
            }
            else
            {
                LogError(functionName, "No API member " + functionName, args);
            }
        }

        /// <summary>
        /// Bundle some debug data together into a printable statement.
        ///
        /// In Unity, print it using LogUtil.
        /// In C#-.NET outside of Unity, print it using Console.
        /// 
        /// </summary>
        /// <param name="functionName">API member that we tried to call</param>
        /// <param name="context">When we detect an issue, report the issue</param>
        /// <param name="args">Args that might have been involved in the issue</param>
        private static void LogError(string functionName, string context, params object[] args)
        {
            // build up a log of what was going on recently
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (object o in args)
            {
                sb.AppendFormat("{0}, ", o);
            }
            string argCat = sb.ToString();

            string printableLogError = string.Format("When calling API member {0} encountered issue: {1}. Args were {2}", functionName, context, argCat);
#if UNITY_5_3_OR_NEWER
            // assume that anywhere that Unity is running, LeiaLoft is also running
            LogUtil.Log(LogLevel.Error, printableLogError);
#else
            // try to keep things compatible with standard C#-.NET
            Console.Error.WriteLine(printableLogError);
#endif
        }

        /// <summary>
        /// Helper function for marshaling data to a C# T[] when given an IntPtr to a C++ array (T*)
        /// </summary>
        /// <typeparam name="T">Type of array to create. Currently only support int, float, IntPtr</typeparam>
        /// <param name="ptr">A pointer to a chunk of C++ memory</param>
        /// <param name="len">Length of array to populate</param>
        /// <returns>An array of T which has elements copied from C++ into C#</returns>
        private static T[] MarshalToGeneric<T>(IntPtr ptr, int len)
        {
            TypeCode tCode = Type.GetTypeCode(typeof(T));
            if (new[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 }.Contains(tCode))
            {
                int[] data = new int[len];
                Marshal.Copy(ptr, data, 0, len);
                return (T[])(object)data;
            }
            else if (tCode == TypeCode.Single)
            {
                float[] data = new float[len];
                Marshal.Copy(ptr, data, 0, len);
                return (T[])(object)data;
            }
            else if (tCode == TypeCode.Object && typeof(T) == typeof(IntPtr))
            {
                IntPtr[] data = new IntPtr[len];
                Marshal.Copy(ptr, data, 0, len);
                return (T[])(object)data;
            }

            else
            {
                LogError("MarshalToGeneric " + ptr.ToString(), "Unsupported type " + typeof(T));
                return default(T[]);
            }
        }
    }
}
