using System.Linq;
using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

namespace LeiaLoft
{
	public static class EditorRenderPipelineUtils
	{
#if UNITY_2020_2_OR_NEWER
		private const string HDRP_PACKAGE = "render-pipelines.high-definition";
		private const string URP_PACKAGE = "render-pipelines.universal";

		private const string TAG_HDRP = "LEIA_HDRP_DETECTED";
		private const string TAG_URP = "LEIA_URP_DETECTED";

		[UnityEditor.Callbacks.DidReloadScripts]
		public static void OnScriptsReloaded()
		{
			CheckForRenderPipelines();
		}
		private static void CheckForRenderPipelines()
		{
			ListRequest request = Client.List(true);
			BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;

			WaitforPackageLoad(request);
			var packagesList = request.Result.ToList();

			bool hasHDRP = packagesList.Find(x => x.name.Contains(HDRP_PACKAGE)) != null;
			bool hasURP = packagesList.Find(x => x.name.Contains(URP_PACKAGE)) != null;

			ValidateCompileDefinition(hasURP, TAG_URP, platform);
			ValidateCompileDefinition(hasHDRP, TAG_HDRP, platform);
		}
		private static void ValidateCompileDefinition(bool passContition, string tag, BuildTargetGroup platform)
        {
            if (passContition) { 
				CompileDefineUtil.AddCompileDefine(platform, tag); 
			}
            else { 
				CompileDefineUtil.RemoveCompileDefine(tag, new[] { platform }); 
			}
        }
		private static void WaitforPackageLoad(ListRequest request)
        {
			if (request == null) { return; }
			for (int i = 0; i < 1000; i++)
			{
				if (request.Result != null) { break; }
				Task.Delay(10).Wait();
			}
			if (request.Result == null)
			{
				LogUtil.Log(LogLevel.Error, "Timeout Exception in requesting packages!");
				return;
			}
		}
#endif
	}
}
